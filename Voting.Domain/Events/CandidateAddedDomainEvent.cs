namespace Voting.Domain.Events
{
    public sealed record CandidateAddedDomainEvent(
        uint SessionId,
        uint CandidateId, 
        string Name
    );
}
