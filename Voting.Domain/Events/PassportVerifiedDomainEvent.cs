using Voting.Domain.Common;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Events
{
    public sealed record PassportVerifiedDomainEvent(
        Guid UserId,
        PassportIdentifier Passport  //мб
    ) : IDomainEvent;
}