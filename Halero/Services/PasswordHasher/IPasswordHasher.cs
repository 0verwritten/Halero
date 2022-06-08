namespace Halero.Services;

public interface IPasswordHasher{
    string GetHash(string password);
    bool VerifyPassword(string passwordHash, string password);
}