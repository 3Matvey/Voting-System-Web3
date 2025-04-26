using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Events
{
    public sealed record SessionCreatedDomainEvent(
        uint SessionId,
        //string SessionAdmin,
        Guid AdminUserId,
        RegistrationMode Mode
    );
}
