using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record VoterRegisteredDomainEvent(
        uint SessionId,
        Guid UserId,
        string Address
    ) : IDomainEvent;
}
