// Voting.Domain.Aggregates/VotingSessionAggregate.cs
using System;
using System.Collections.Generic;
using Voting.Domain.Entities;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;

namespace Voting.Domain.Aggregates
{
    /// <summary>
    /// Агрегат–проекция состояния on-chain VotingSession.
    /// Восстанавливается только через Apply-методы при поступлении событий.
    /// </summary>
    public class VotingSessionAggregate
    {
        public uint SessionId { get; private set; }
        public RegistrationMode Mode { get; private set; }
        public string Admin { get; private set; } = string.Empty;
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly Dictionary<uint, Candidate> _candidates = new();
        private readonly HashSet<string> _voters = new();

        /// <summary>
        /// Пустой конструктор для event-sourcing’а.
        /// </summary>
        public VotingSessionAggregate() { }

        public void Apply(SessionCreatedDomainEvent e)
        {
            if (SessionId != 0 && SessionId != e.SessionId)
                throw new DomainException(
                    $"Несоответствие SessionId: в памяти={SessionId}, в событии={e.SessionId}");
            SessionId = e.SessionId;
            Mode = e.Mode;
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
            if (_candidates.TryGetValue(e.CandidateId, out var candidate))
            {
                candidate.IncrementVote();
                _candidates[e.CandidateId] = candidate;
            }
            _voters.Add(e.Voter);
        }

        // Дополнительные акссесоры:
        public IReadOnlyCollection<Candidate> GetCandidates() => _candidates.Values;
        public bool HasVoted(string voter) => _voters.Contains(voter);
    }
}
