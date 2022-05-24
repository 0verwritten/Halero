namespace Halero.Services;

interface IPasswordHasher{
    string GetHash(string password);
    bool VerifyPassword(string passwordHash, string password);
}