using Voting.Domain.Exceptions;

namespace Voting.Domain.Entities
{
    /// <summary>Класс сессии голосования.</summary>
    public class VotingSession(string sessionAdmin)
    {
        /// <summary>Уникальный идентификатор сессии.</summary>
        public Guid SessionId { get; } = Guid.NewGuid();

        /// <summary>Адрес администратора сессии.</summary>
        public string SessionAdmin { get; } = sessionAdmin;

        /// <summary>Время начала голосования (UTC).</summary>
        public DateTime StartTime { get; private set; }

        /// <summary>Время окончания голосования (UTC).</summary>
        public DateTime EndTime { get; private set; }

        /// <summary>Флаг активности голосования.</summary>
        public bool VotingActive { get; private set; } = false;

        /// <summary>Счётчик добавленных кандидатов для генерации уникального идентификатора. </summary>
        public int CandidatesAddedCount { get; private set; } = 0;

        /// <summary>Список активных кандидатов.</summary>
        public List<Candidate> Candidates { get; } = [];

        /// <summary>Множество адресов, проголосовавших в сессии.</summary>
        public HashSet<string> HasVoted { get; } = [];

        /// <summary>
        /// Проверяет, что голосование не активно, чтобы можно было менять кандидатов.
        /// </summary>
        private void EnsureVotingNotActive()
        {
            if (VotingActive)
                throw new DomainException("Нельзя изменять кандидатов во время активного голосования");
        }

        /// <summary>
        /// Добавляет кандидата в сессию.
        /// </summary>
        /// <param name="name">Имя кандидата.</param>
        public void AddCandidate(string name)
        {
            EnsureVotingNotActive();
            CandidatesAddedCount++;
            var candidate = new Candidate(CandidatesAddedCount, name);
            Candidates.Add(candidate);
        }

        /// <summary>
        /// Удаляет кандидата по идентификатору.
        /// </summary>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        public void RemoveCandidate(int candidateId)
        {
            EnsureVotingNotActive();
            var candidate = Candidates.FirstOrDefault(c => c.Id == candidateId)
                ?? throw new DomainException("Кандидат не найден");
            Candidates.Remove(candidate);
        }

        /// <summary>
        /// Запускает голосование на заданное время (в минутах).
        /// </summary>
        /// <param name="durationMinutes">Длительность голосования (в минутах).</param>
        public void StartVoting(int durationMinutes)
        {
            if (VotingActive)
                throw new DomainException("Голосование уже активно");
            if (Candidates.Count < 2)
                throw new DomainException("Необходимо минимум два активных кандидата для начала голосования");

            StartTime = DateTime.UtcNow;
            EndTime = StartTime.AddMinutes(durationMinutes);
            VotingActive = true;
        }

        /// <summary>
        /// Вспомогательный метод для проверки условий голосования для конкретного избирателя.
        /// </summary>
        /// <param name="voterAddress">Адрес голосующего.</param>
        private void ValidateVotingEligibility(string voterAddress)
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");
            if (DateTime.UtcNow < StartTime || DateTime.UtcNow > EndTime)
                throw new DomainException("Время голосования не активно");
            if (HasVoted.Contains(voterAddress))
                throw new DomainException("Пользователь уже голосовал");
        }

        /// <summary>
        /// Регистрирует голос за кандидата.
        /// </summary>
        /// <param name="voterAddress">Адрес голосующего.</param>
        /// <param name="candidateId">Идентификатор кандидата.</param>
        public void Vote(string voterAddress, int candidateId)
        {
            ValidateVotingEligibility(voterAddress);
            var candidate = Candidates.FirstOrDefault(c => c.Id == candidateId)
                ?? throw new DomainException("Некорректный идентификатор кандидата");

            HasVoted.Add(voterAddress);
            candidate.IncrementVote();
        }

        /// <summary>
        /// Завершает голосование.
        /// </summary>
        public void EndVoting()
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");

            VotingActive = false;
            EndTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Возвращает общее количество голосов в сессии.
        /// </summary>
        public int GetTotalVotesCount() =>
            Candidates.Sum(c => c.VoteCount);
    }
}
