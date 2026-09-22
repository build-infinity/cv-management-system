
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CvManagementSystem.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}