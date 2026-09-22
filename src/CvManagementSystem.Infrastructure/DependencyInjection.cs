
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Infrastructre.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CvManagementSystem.Infrastructre;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}