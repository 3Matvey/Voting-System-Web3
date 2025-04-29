using Voting.Domain.Common;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Events
{
    public sealed record SessionCreatedDomainEvent(
        uint SessionId,
        Guid AdminUserId,
        RegistrationMode Mode,
        VerificationLevel RequiredVerificationLevel
    ) : IDomainEvent;
}
