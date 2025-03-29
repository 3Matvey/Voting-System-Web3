using Voting.Domain.Entities;

namespace Voting.Domain.Interfaces
{
    /// <summary>Интерфейс для взаимодействия с блокчейном.</summary>
    public interface IBlockchainService
    {
        /// <summary>Добавляет кандидата в блокчейне.</summary>
        /// <param name="candidateName">Имя кандидата.</param>
        /// <returns>Идентификатор транзакции или другой результат операции.</returns>
        Task<string> AddCandidateAsync(string candidateName);

        /// <summary>Запускает голосование в блокчейне с заданной длительностью (в минутах).</summary>
        /// <param name="durationMinutes">Длительность голосования в минутах.</param>
        /// <returns>Идентификатор транзакции или результат запуска голосования.</returns>
        Task<string> StartVotingAsync(int durationMinutes);

        /// <summary>Регистрирует голосование за кандидата в блокчейне.</summary>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        /// <param name="voterAddress">Адрес голосующего.</param>
        /// <returns>Идентификатор транзакции или результат голосования.</returns>
        Task<string> VoteAsync(int candidateId, string voterAddress);

        /// <summary>Завершает голосование в блокчейне.</summary>
        /// <returns>Идентификатор транзакции или результат завершения голосования.</returns>
        Task<string> EndVotingAsync();

        /// <summary>Получает результаты голосования из блокчейна.</summary>
        /// <returns>Список кандидатов с их голосами.</returns>
        Task<IEnumerable<Candidate>> GetResultsAsync();
    }
}
