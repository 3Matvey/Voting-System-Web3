using Voting.Domain.Events;
using Voting.Domain.Exceptions;

namespace Voting.Domain.Aggregates
{
    public class VotingSessionAggregate(uint sessionId)
    {
        public uint SessionId { get; } = sessionId;
        public string Admin { get; private set; } = string.Empty;
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly Dictionary<uint, Candidate> _candidates = [];

        private readonly HashSet<string> _voters = [];

        public void Apply(SessionCreatedDomainEvent e)
        {
            // SessionId совпадает по конструктору
            // здесь можно проверить e.SessionId == SessionId, либо довериться
            if (SessionId != e.SessionId)
                throw new DomainException($"Несоответствие идентификаторов между моделью в памяти ({nameof(SessionId)} = {SessionId}) и onChain моделью({nameof(e.SessionId)} = {e.SessionId})");
            Admin = e.SessionAdmin;
        }

        public void Apply(CandidateAddedDomainEvent e)
        {
            _candidates[e.CandidateId] = new Candidate(e.CandidateId, e.Name, 0);
        }

        public void Apply(CandidateRemovedDomainEvent e)
        {
            _candidates.Remove(e.CandidateId);
        }

        public void Apply(VotingStartedDomainEvent e)
        {
            VotingActive = true;
            StartTimeUtc = e.StartTimeUtc;
            EndTimeUtc = e.EndTimeUtc;
        }

        public void Apply(VotingEndedDomainEvent e)
        {
            VotingActive = false;
            EndTimeUtc = e.EndTimeUtc;
        }

        public void Apply(VoteCastDomainEvent e)
        {
            if (_candidates.TryGetValue(e.CandidateId, out var c))
                c.IncrementVote();
            _voters.Add(e.Voter);
        }
    }
}
