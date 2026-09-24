using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Infrastructure.Security;
using CvManagementSystem.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CvManagementSystem.Infrastructure.Persistence;
using CvManagementSystem.Infrastructure.Persistence.Repositories;

namespace CvManagementSystem.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection("Smtp"));
        services.AddOptions<EmailVerificationOptions>().Bind(configuration.GetSection("EmailVerification"));
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<EmailBackgroundService>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddOptions<GoogleAuthOptions>().Bind(configuration.GetSection("Google"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddCookie(AuthCookies.ExternalScheme, options =>
            {
                options.Cookie.Name = AuthCookies.ExternalCookie;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            })
            .AddGoogle(options =>
            {
                var google = configuration.GetSection("Google").Get<GoogleAuthOptions>()!;
                options.ClientId = google.ClientId;
                options.ClientSecret = google.ClientSecret;
                options.SignInScheme = AuthCookies.ExternalScheme;
                options.CallbackPath = "/signin-google";
                options.UsePkce = true;
                options.SaveTokens = false;
                options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
                options.ClaimActions.MapJsonKey(AuthCookies.GoogleEmailVerifiedClaim, "email_verified");
                options.Events.OnRemoteFailure = async context =>
                {
                    context.HandleResponse();
                    await context.HttpContext.SignOutAsync(AuthCookies.ExternalScheme);
                    if (context.Properties?.RedirectUri == "/login.html?google=callback")
                    {
                        context.Response.Redirect("/login.html?google=error");
                        return;
                    }
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Google authentication failed or was cancelled." });
                };
            });
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var settings = jwtOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = "sub",
                    ClockSkew = TimeSpan.Zero
                };
            });
        services.AddAuthorization();

        return services;
    }
}
