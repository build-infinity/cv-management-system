using CvManagementSystem.Domain.Entities;

namespace CvManagementSystem.Application.Abstractions;
public interface IEmailVerificationService
{
    void Queue(User user);
    bool IsTokenValid(User user, string token);
}
