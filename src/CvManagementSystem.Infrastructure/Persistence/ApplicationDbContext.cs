using CvManagementSystem.Domain.Entities;
using CvManagementSystem.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CvManagementSystem.Infrastructure.Persistence;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
