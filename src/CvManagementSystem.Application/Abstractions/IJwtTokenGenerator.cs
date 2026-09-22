using CvManagementSystem.Domain.Entities;

namespace CvManagementSystem.Application.Abstractions;
public interface IJwtTokenGenerator
{
    string Generate(User user);
}
