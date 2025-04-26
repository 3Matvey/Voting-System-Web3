// Voting.Domain.Aggregates/VotingSessionAggregate.cs
using System;
using System.Collections.Generic;
using Voting.Domain.Entities;
using Voting.Domain.Entities.ValueObjects;
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
        public Guid AdminUserId { get; private set; }
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly Dictionary<uint, Candidate> _candidates = new();
        private readonly HashSet<Guid> _votedUserIds = new();

        public VotingSessionAggregate() { }

        public void Apply(SessionCreatedDomainEvent e)
        {
            if (SessionId != 0 && SessionId != e.SessionId)
                throw new DomainException(
                    $"Несоответствие SessionId: в памяти={SessionId}, в событии={e.SessionId}");
            SessionId = e.SessionId;
            Mode = e.Mode;
            AdminUserId = e.AdminUserId;
        }

        public void Apply(CandidateAddedDomainEvent e)
            => _candidates[e.CandidateId] = new Candidate(e.CandidateId, e.Name, 0);

        public void Apply(CandidateRemovedDomainEvent e)
            => _candidates.Remove(e.CandidateId);

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
            _votedUserIds.Add(e.VoterId);
        }

        public void UpdateCandidateDescription(uint candidateId, string newDescription)
        {
            if (SessionId == 0)
                throw new DomainException("Session not initialized");
            if (!_candidates.TryGetValue(candidateId, out var candidate))
                throw new DomainException($"Candidate {candidateId} not found");
            if (VotingActive)
                throw new DomainException("Нельзя менять описание во время активного голосования");

            candidate.UpdateDescription(newDescription);
            _candidates[candidateId] = candidate;
        }

        public IReadOnlyCollection<Candidate> GetCandidates() => _candidates.Values;
        public bool HasVoted(Guid userId) => _votedUserIds.Contains(userId);
    }
}
