using Microsoft.Extensions.DependencyInjection;
using CvManagementSystem.Application.Interfaces;
using CvManagementSystem.Application.Services;

namespace CvManagementSystem.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
