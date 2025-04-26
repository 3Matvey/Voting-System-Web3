using Voting.Domain.Entities;

namespace Voting.Domain.Events
{
    public sealed record SessionCreatedDomainEvent(uint SessionId, string SessionAdmin, RegistrationMode Mode);
    public sealed record CandidateAddedDomainEvent(uint SessionId, uint CandidateId, string Name);
    public sealed record CandidateRemovedDomainEvent(uint SessionId, uint CandidateId);
    public sealed record VotingStartedDomainEvent(uint SessionId, DateTime StartTimeUtc, DateTime EndTimeUtc);
    public sealed record VotingEndedDomainEvent(uint SessionId, DateTime EndTimeUtc);
    public sealed record VoteCastDomainEvent(uint SessionId, string Voter, uint CandidateId);
}
