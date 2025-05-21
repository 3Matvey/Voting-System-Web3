using System.Security.Cryptography;
using Voting.Application.Interfaces;

namespace Voting.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;      // 128 bit
        private const int KeySize = 32;      // 256 bit
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public (string Hash, string Salt) Hash(string password)
        {
            // генерируем случайную соль
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            // вычисляем PBKDF2-хеш
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                Algorithm,
                KeySize
            );
            return (
                Convert.ToBase64String(hashBytes),
                Convert.ToBase64String(saltBytes)
            );
        }

        public bool Verify(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                Algorithm,
                KeySize
            );
            // Безопасное сравнение
            return CryptographicOperations.FixedTimeEquals(
                hashBytes,
                Convert.FromBase64String(storedHash)
            );
        }
    }
}
