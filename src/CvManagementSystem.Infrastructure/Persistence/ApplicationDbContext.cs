using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvManagementSystem.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
