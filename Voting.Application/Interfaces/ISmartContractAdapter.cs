using Voting.Domain.Entities;

namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для взаимодействия с смарт-контрактом голосования.
    /// Реализуется адаптером, который инкапсулирует детали вызова методов контракта.
    /// </summary>
    public interface ISmartContractAdapter
    {
        /// <summary>
        /// Создаёт новую сессию голосования в блокчейне.
        /// </summary>
        /// <param name="sessionAdmin">Адрес администратора сессии.</param>
        /// <returns>Идентификатор созданной сессии (например, номер сессии).</returns>
        Task<uint> CreateSessionAsync(string sessionAdmin, CancellationToken ct = default);

        /// <summary>
        /// Добавляет кандидата в сессию голосования.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateName">Имя кандидата.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> AddCandidateAsync(uint sessionId, string candidateName, CancellationToken ct = default);

        /// <summary>
        /// Удаляет кандидата из сессии голосования.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> RemoveCandidateAsync(uint sessionId, uint candidateId, CancellationToken ct = default);

        /// <summary>
        /// Запускает голосование в сессии на заданное время (в минутах).
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="durationMinutes">Длительность голосования в минутах.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> StartVotingAsync(uint sessionId, uint durationMinutes, CancellationToken ct = default);

        /// <summary>
        /// Регистрирует голосование за кандидата.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <param name="voterAddress">Адрес голосующего.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> VoteAsync(uint sessionId, uint candidateId, User user, CancellationToken ct = default);

        /// <summary>
        /// Завершает голосование в сессии.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> EndVotingAsync(uint sessionId, CancellationToken ct = default);

        /// <summary>
        /// Получает статус голосования по сессии.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <returns>
        /// Кортеж, содержащий:
        /// isActive: true, если голосование активно;
        /// timeLeft: оставшееся время голосования (в секундах);
        /// totalVotesCount: общее количество голосов в сессии.
        /// </returns>
        Task<(bool isActive, uint timeLeft, uint totalVotesCount)> GetVotingStatusAsync(uint sessionId, CancellationToken ct = default);

        /// <summary>
        /// Получает список кандидатов и их текущее количество голосов.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <returns>Список кандидатов с соответствующими голосами.</returns>
        Task<IEnumerable<Candidate>> GetCandidatesAsync(uint sessionId, CancellationToken ct = default);

        /// <summary>
        /// Получает кандидата по идентификатору сессии и кандидата.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <returns></returns>
        Task<Candidate> GetCandidateAsync(uint sessionId, uint candidateId, CancellationToken ct = default);
    }
}
