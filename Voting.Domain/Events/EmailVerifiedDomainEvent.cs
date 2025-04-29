using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record EmailVerifiedDomainEvent(
        Guid UserId
    ) : IDomainEvent;
}