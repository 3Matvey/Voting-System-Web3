using Voting.Domain.Common;

namespace Voting.Domain.Events
{
    public sealed record VoteCastDomainEvent(
        uint SessionId,
        //string Voter,
        Guid VoterId,
        uint CandidateId
    ) : IDomainEvent;
}
