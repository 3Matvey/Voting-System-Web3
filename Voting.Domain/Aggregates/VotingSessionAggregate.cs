using Voting.Domain.Common;
using Voting.Domain.Entities;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Aggregates
{
    public sealed class VotingSessionAggregate : AggregateRoot
    {
        private uint _id;
        public uint Id
        {
            get => _id;
            private set
            {
                if (_id == 0)
                    _id = value;
                else if (_id != value)
                    throw new DomainException($"Attempt to overwrite the Id in the session {_id} to {value}");
            }
        }

        public RegistrationMode Mode { get; private set; }
        public VerificationLevel RequiredVerificationLevel { get; private set; }
        public Guid AdminUserId { get; private set; }
        public bool VotingActive { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public DateTime? EndTimeUtc { get; private set; }

        private readonly List<Candidate> _candidates = new();
        public ICollection<Candidate> Candidates => _candidates;

        private readonly List<Guid> _votedUserIds = [];
        private readonly List<Guid> _registeredUserIds = [];

        public IReadOnlyCollection<Guid> RegisteredUserIds => _registeredUserIds.AsReadOnly();
        public IReadOnlyCollection<Guid> VotedUserIds => _votedUserIds.AsReadOnly();

        public VotingSessionAggregate() { }

        public bool HasVoted(Guid userId) => _votedUserIds.Contains(userId);

        #region Apply
        public VotingSessionAggregate Apply(SessionCreatedDomainEvent e)
        {
            if (e.SessionId != 0) 
                Id = e.SessionId;
            if (e.AdminUserId != Guid.Empty) 
                AdminUserId = e.AdminUserId;
            if (e.Mode != default) 
                Mode = e.Mode;
            if (e.RequiredVerificationLevel != default)
                RequiredVerificationLevel = e.RequiredVerificationLevel;

            return this;
        }

        public VotingSessionAggregate Apply(CandidateAddedDomainEvent e)
        {
            if (_candidates.All(c => c.Id != e.CandidateId))
                _candidates.Add(new Candidate(e.CandidateId, e.Name, 0));

            return this;
        }

        public VotingSessionAggregate Apply(CandidateRemovedDomainEvent e)
        {
            _candidates.RemoveAll(c => c.Id == e.CandidateId);
            return this;
        }

        public VotingSessionAggregate Apply(VotingStartedDomainEvent e)
        {
            VotingActive = true;
            StartTimeUtc = e.StartTimeUtc;
            EndTimeUtc = e.EndTimeUtc;
            return this;
        }

        public VotingSessionAggregate Apply(VotingEndedDomainEvent e)
        {
            VotingActive = false;
            EndTimeUtc = e.EndTimeUtc;
            return this;
        }

        public VotingSessionAggregate Apply(VoteCastDomainEvent e)
        {
            // ищем кандидата и инкрементим
            var candidate = _candidates.SingleOrDefault(c => c.Id == e.CandidateId);
            if (candidate != null)
                candidate.IncrementVote();

            _votedUserIds.Add(e.VoterId);
            return this;
        }

        public VotingSessionAggregate Apply(VoterRegisteredDomainEvent e)
        {
            _registeredUserIds.Add(e.UserId);
            return this;
        }

        public VotingSessionAggregate Apply(CandidateDescriptionUpdatedDomainEvent e)
        {
            var candidate = _candidates.SingleOrDefault(c => c.Id == e.CandidateId);
            if (candidate is null)
                throw new DomainException($"Candidate {e.CandidateId} not found");
            if (VotingActive)
                throw new DomainException("Cannot update description during active voting");

            candidate.UpdateDescription(e.NewDescription);

            // повторно эмитим событие в DomainEvents, если нужно
            AddDomainEvent(new CandidateDescriptionUpdatedDomainEvent(
                Id,
                e.CandidateId,
                e.NewDescription
            ));

            return this;
        }
        #endregion

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

            var @event = new VoterRegisteredDomainEvent(Id, voter.Id, voter.BlockchainAddress);
            AddDomainEvent(@event);
            Apply(@event);
        }
    }
}
