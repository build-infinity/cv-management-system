namespace CvManagementSystem.Application.Abstractions;
public interface IPasswordHasher
{
    string Hash(string passsword);
    bool Verify(string passsword, string hashPaswword);
}