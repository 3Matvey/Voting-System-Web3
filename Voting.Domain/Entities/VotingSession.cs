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

        /// <summary>Последний присвоенный идентификатор кандидата.</summary>
        public int CandidatesCount { get; private set; } = 0;

        /// <summary>Фактическое число активных кандидатов.</summary>
        public int ActiveCandidatesCount { get; private set; } = 0;

        /// <summary>Список кандидатов сессии.</summary>
        public List<Candidate> Candidates { get; } = [];

        /// <summary>Множество адресов, проголосовавших в сессии.</summary>
        public HashSet<string> HasVoted { get; } = [];

        /// <summary>Добавляет кандидата в сессию.</summary>
        /// <param name="name"> Имя кандидата.</param>
        public void AddCandidate(string name)
        {
            if (VotingActive)
                throw new DomainException("Нельзя добавлять кандидатов во время активного голосования");

            CandidatesCount++;
            ActiveCandidatesCount++;
            var candidate = new Candidate(CandidatesCount, name);
            Candidates.Add(candidate);
        }

        /// <summary>Удаляет кандидата из сессии по идентификатору.</summary>
        public void RemoveCandidate(int candidateId)
        {
            if (VotingActive)
                throw new DomainException("Нельзя удалять кандидатов во время активного голосования");

            var candidate = Candidates.FirstOrDefault(c => c.Id == candidateId)
                ?? throw new DomainException("Кандидат не найден");

            Candidates.Remove(candidate);
            ActiveCandidatesCount--;
        }

        /// <summary>Запускает голосование с заданной длительностью (в минутах).</summary>
        /// <param name="durationMinutes"> Длительность голосования (в минутах).</param>
        public void StartVoting(int durationMinutes)
        {
            if (VotingActive)
                throw new DomainException("Голосование уже активно");
            if (ActiveCandidatesCount < 2)
                throw new DomainException("Необходимо минимум два активных кандидата для начала голосования");

            StartTime = DateTime.UtcNow;
            EndTime = StartTime.AddMinutes(durationMinutes);
            VotingActive = true;
        }

        /// <summary>Регистрирует голос от пользователя за указанного кандидата.</summary>
        /// <param name="candidateId"> Идентификатор кандидата. </param>
        /// <param name="voterAddress"> Адрес голосующего. </param> 
        public void Vote(string voterAddress, int candidateId)
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");
            if (DateTime.UtcNow < StartTime || DateTime.UtcNow > EndTime)
                throw new DomainException("Время голосования не активно");
            if (HasVoted.Contains(voterAddress))
                throw new DomainException("Пользователь уже голосовал");

            var candidate = Candidates.FirstOrDefault(c => c.Id == candidateId)
                ?? throw new DomainException("Некорректный идентификатор кандидата");

            HasVoted.Add(voterAddress);
            candidate.IncrementVote();
        }

        /// <summary>Завершает голосование.</summary>
        public void EndVoting()
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");

            VotingActive = false;
            EndTime = DateTime.UtcNow;
        }

        /// <summary>Возвращает общее количество голосов в сессии.</summary>
        public int GetTotalVotesCount() =>
            Candidates.Sum(c => c.VoteCount);
    }
}
