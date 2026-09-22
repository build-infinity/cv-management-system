using CvManagementSystem.Domain.Entities;

namespace CvManagementSystem.Application.Abstractions;
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
