namespace Voting.Domain.Events
{
    public record VoterRegisteredDomainEvent(
        uint SessionId,
        Guid UserId,
        string Address
    );
}
