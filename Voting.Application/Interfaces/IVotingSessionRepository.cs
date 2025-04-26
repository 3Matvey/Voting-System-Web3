using Voting.Domain.Entities;

namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для off-chain конфигурации сессии голосования.
    /// </summary>
    public interface IVotingSessionRepository : IAsyncDisposable
    {
        /// <summary>
        /// Сохранить базовую конфигурацию новой сессии 
        /// (должно вызываться сразу после вызова createSession() on-chain).
        /// </summary>
        Task AddSessionConfigAsync(uint sessionId, RegistrationMode mode, string admin);

        /// <summary>
        /// Получить сохранённый режим регистрации для сессии.
        /// </summary>
        Task<RegistrationMode?> GetRegistrationModeAsync(uint sessionId);
    }
}
