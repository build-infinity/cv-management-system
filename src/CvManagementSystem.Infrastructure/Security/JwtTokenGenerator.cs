using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CvManagementSystem.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IOptions<JwtOptions> _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options;
    }

    public string Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = _options.Value;
        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            notBefore: now,
            expires: now.AddMinutes(settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
