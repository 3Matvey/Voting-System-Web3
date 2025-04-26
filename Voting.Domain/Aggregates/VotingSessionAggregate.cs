// Voting.Domain.Aggregates/VotingSessionAggregate.cs
using System;
using System.Collections.Generic;
using Voting.Domain.Entities;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;

namespace Voting.Domain.Aggregates
{
    /// <summary>
    /// Агрегат-проекция состояния сессии голосования.
    /// Состояние полностью восстанавливается через Apply-методы при поступлении событий контракта.
    /// </summary>
    public class VotingSessionAggregate
    {
        public uint SessionId { get; private set; }
        public string Admin { get; private set; } = string.Empty;
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly Dictionary<uint, Candidate> _candidates = new();
        private readonly HashSet<string> _voters = new();

        /// <summary>
        /// Пустой конструктор для восстановления из событий.
        /// </summary>
        public VotingSessionAggregate() { }

        public void Apply(SessionCreatedDomainEvent e)
        {
            if (SessionId == 0)
                throw new DomainException(
                    $"SessionId не должен быть равен 0");
            if (SessionId != e.SessionId)
                throw new DomainException(
                    $"Несоответствие SessionId: в памяти={SessionId}, в событии={e.SessionId}");
            SessionId = e.SessionId;
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

        // Публичные методы чтения состояния (Getters) можно добавить по необходимости,
        // например: IReadOnlyCollection<Candidate> GetCandidates(), bool HasVoted(address), и т.п.
    }
}
