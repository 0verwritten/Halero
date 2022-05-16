using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Halero.Services;

class SHA512PasswordHasher : IPasswordHasher{
    public string GetHash(string password){

        byte[] salt = new byte[256 / 8];
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetNonZeroBytes(salt);
        }

        return GetHashWithSalt(password, salt);
    }

    string GetHashWithSalt(string password, byte[] salt){
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: 100000,
            numBytesRequested: 512 / 8));

        return Convert.ToBase64String( Encoding.UTF8.GetBytes($"{hashed}.{Encoding.Default.GetString(salt)}") );
    }

    public bool VerifyPassword(string passwordHash, string password){
        var passwordData = Encoding.UTF8.GetString(
                                        Convert.FromBase64String(passwordHash)
                                    ).Split(".");

        string salt = passwordData[1];
        passwordHash = passwordData[0];

        return GetHashWithSalt(password, Encoding.Default.GetBytes(salt)) == passwordHash;
    }
}