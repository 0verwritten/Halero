using System.Security.Cryptography;
using System.Text;

namespace Halero.Services;

public class MD5PasswordHasher : IPasswordHasher{
    public string GetHash(string input)
    {
        MD5 hasher = MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = hasher.ComputeHash(inputBytes);
        
        StringBuilder buildingString = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            buildingString.Append(hashBytes[i].ToString("X2"));
        }
        return buildingString.ToString();
    }

    public bool VerifyPassword(string passwordHash, string password) => passwordHash == GetHash(password);
}