using CvManagementSystem.Application.Interfaces;
using CvManagementSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CvManagementSystem.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
