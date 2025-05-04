using Voting.Domain.Common;
using Voting.Domain.Entities;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Aggregates
{
    public sealed class VotingSessionAggregate : AggregateRoot<uint>
    {
        public override uint Id { get; private protected set; }
        public RegistrationMode Mode { get; private set; }
        public VerificationLevel RequiredVerificationLevel { get; private set; }
        public Guid AdminUserId { get; private set; }
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly Dictionary<uint, Candidate> _candidates = [];
        private readonly HashSet<Guid> _votedUserIds = [];


        public VotingSessionAggregate() { }

        public IReadOnlyCollection<Candidate> GetCandidates() => _candidates.Values;
        public bool HasVoted(Guid userId) => _votedUserIds.Contains(userId);

        #region ON-CHAIN 
        public void Apply(SessionCreatedDomainEvent e)
        {
            if (Id != 0 && Id != e.SessionId)
                throw new DomainException($"SessionId mismatch: in-memory={Id}, event={e.SessionId}");
            Id = e.SessionId;
            Mode = e.Mode;
            AdminUserId = e.AdminUserId;
            RequiredVerificationLevel = e.RequiredVerificationLevel;
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
        #endregion

        // OFF-CHAIN команда — регистрируем юзера на сессию
        public void RegisterVoter(User voter)
        {
            if (Id == 0)
                throw new DomainException("Session not initialized");

            if (voter.VerificationLevel < RequiredVerificationLevel)
                throw new DomainException(
                    $"User {voter.Id} has insufficient verification: " +
                    $"{voter.VerificationLevel}, required: {RequiredVerificationLevel}");

            if (_votedUserIds.Contains(voter.Id))
                throw new DomainException($"User {voter.Id} is already registered or has voted");

            // аккумулируем событие регистрации
            var @event = new VoterRegisteredDomainEvent(Id, voter.Id, voter.BlockchainAddress);
            AddDomainEvent(@event);
            Apply(@event);
        }

        public void Apply(VoterRegisteredDomainEvent e)
        {
            _votedUserIds.Add(e.UserId);
        }

        // Команда на изменение описания (как до этого)
        public void UpdateCandidateDescription(uint candidateId, string newDescription)
        {
            if (Id == 0)
                throw new DomainException("Session not initialized");
            if (!_candidates.TryGetValue(candidateId, out var candidate))
                throw new DomainException($"Candidate {candidateId} not found");
            if (VotingActive)
                throw new DomainException("Cannot update description during active voting");

            candidate.UpdateDescription(newDescription);
            _candidates[candidateId] = candidate;


            AddDomainEvent(new CandidateDescriptionUpdatedDomainEvent(
                Id,
                candidateId,
                newDescription
            ));
        }
    }
}
