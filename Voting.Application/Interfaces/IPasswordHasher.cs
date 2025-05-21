// Voting.Application/Interfaces/IPasswordHasher.cs
namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Хеширует и проверяет пароль.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <returns>Tuple, где Hash — Base64-строка хеша, Salt — Base64-строка соли</returns>
        (string Hash, string Salt) Hash(string password);

        /// <returns>true, если пароль при указанной соли даёт тот же хеш</returns>
        bool Verify(string password, string storedHash, string storedSalt);
    }
}
