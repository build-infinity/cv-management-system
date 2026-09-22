using CvManagementSystem.Application.Abstractions;


namespace CvManagementSystem.Infrastructre.Security;
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string passsword)
    {
        return BCrypt.Net.BCrypt.HashPassword(passsword);
    }
    public bool Verify(string passsword, string hashPassword)
    {
        return BCrypt.Net.BCrypt.Verify(passsword, hashPassword);
    }
}