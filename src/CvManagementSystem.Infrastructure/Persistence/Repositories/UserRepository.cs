using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvManagementSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return _context.Users.AsNoTracking().SingleOrDefaultAsync(user => user.GoogleId == googleId, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }
}
