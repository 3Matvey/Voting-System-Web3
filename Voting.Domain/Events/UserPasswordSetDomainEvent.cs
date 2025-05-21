using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record UserPasswordSetDomainEvent(
        Guid Id) : IDomainEvent;
}
