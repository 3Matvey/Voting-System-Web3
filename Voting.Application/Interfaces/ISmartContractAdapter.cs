﻿using Voting.Domain.Entities;

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
        Task<uint> CreateSessionAsync(string sessionAdmin);

        /// <summary>
        /// Добавляет кандидата в сессию голосования.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateName">Имя кандидата.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> AddCandidateAsync(uint sessionId, string candidateName);

        /// <summary>
        /// Удаляет кандидата из сессии голосования.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> RemoveCandidateAsync(uint sessionId, uint candidateId);

        /// <summary>
        /// Запускает голосование в сессии на заданное время (в минутах).
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="durationMinutes">Длительность голосования в минутах.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> StartVotingAsync(uint sessionId, uint durationMinutes);

        /// <summary>
        /// Регистрирует голосование за кандидата.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <param name="voterAddress">Адрес голосующего.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> VoteAsync(uint sessionId, uint candidateId, string voterAddress);

        /// <summary>
        /// Завершает голосование в сессии.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <returns>Идентификатор транзакции или результат операции.</returns>
        Task<string> EndVotingAsync(uint sessionId);

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
        Task<(bool isActive, uint timeLeft, uint totalVotesCount)> GetVotingStatusAsync(uint sessionId);

        /// <summary>
        /// Получает список кандидатов и их текущее количество голосов.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования.</param>
        /// <returns>Список кандидатов с соответствующими голосами.</returns>
        Task<IEnumerable<Candidate>> GetCandidatesAsync(uint sessionId);
    }
}
