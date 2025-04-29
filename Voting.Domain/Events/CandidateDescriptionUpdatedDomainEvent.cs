using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record CandidateDescriptionUpdatedDomainEvent(
        uint SessionId,
        uint CandidateId,
        string NewDescription
    ) : IDomainEvent;
}
