using CvManagementSystem.Application.Abstractions;

namespace CvManagementSystem.Infrastructure.Security;
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public bool Verify(string password, string hashPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashPassword);
    }
}