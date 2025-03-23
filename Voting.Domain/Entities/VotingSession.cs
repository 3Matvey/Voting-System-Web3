using Voting.Domain.Exceptions;

namespace Voting.Domain.Entities
{
    public class VotingSession
    {
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public bool VotingActive { get; private set; }
        public List<Candidate> Candidates { get; private set; } = [];
        public HashSet<string> Voters { get; private set; } = [];// для хранения адресов голосующих

        // Добавление кандидата до начала голосования
        public void AddCandidate(Candidate candidate)
        {
            if (VotingActive)
                throw new DomainException("Нельзя добавить кандидата во время активной сессии голосования");
            Candidates.Add(candidate);
        }

        // Запуск голосования с указанием длительности в минутах
        public void StartVoting(int durationMinutes)
        {
            if (VotingActive)
                throw new DomainException("Голосование уже активно");
            if (Candidates.Count < 2)
                throw new DomainException("Необходимо минимум два кандидата для начала голосования");

            StartTime = DateTime.UtcNow;
            EndTime = StartTime.AddMinutes(durationMinutes);
            VotingActive = true;
        }

        // Голосование за кандидата
        public void Vote(string voterAddress, int candidateId)
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");
            if (DateTime.UtcNow < StartTime || DateTime.UtcNow > EndTime)
                throw new DomainException("Время голосования неактивно");
            if (Voters.Contains(voterAddress))
                throw new DomainException("Адрес уже проголосовал");

            var candidate = Candidates.FirstOrDefault(c => c.Id == candidateId);
            if (candidate == null)
                throw new DomainException("Некорректный идентификатор кандидата");

            candidate.IncrementVote();
            Voters.Add(voterAddress);
        }

        // Завершение голосования
        public void EndVoting()
        {
            if (!VotingActive)
                throw new DomainException("Голосование не активно");
            VotingActive = false;
            EndTime = DateTime.UtcNow;
        }
    }
}
