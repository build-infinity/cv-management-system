using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvManagementSystem.Infrastructure.Persistence.Repositories;
public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return context.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }
}
