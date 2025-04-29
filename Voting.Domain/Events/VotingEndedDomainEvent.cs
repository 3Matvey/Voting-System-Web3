using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record VotingEndedDomainEvent(
        uint SessionId, 
        DateTime EndTimeUtc
    ) : IDomainEvent;
}
