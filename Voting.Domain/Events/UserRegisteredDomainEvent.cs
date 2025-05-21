using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record UserRegisteredDomainEvent(
        Guid UserId,
        string Email) : IDomainEvent;
}
