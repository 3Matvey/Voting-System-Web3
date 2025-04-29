using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record VotingStartedDomainEvent(
        uint SessionId, 
        DateTime StartTimeUtc,
        DateTime EndTimeUtc
    ) : IDomainEvent;
}
