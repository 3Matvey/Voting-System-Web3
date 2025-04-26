namespace Voting.Domain.Events
{
    public sealed record CandidateRemovedDomainEvent(
        uint SessionId,
        uint CandidateId
    );
}
