using Voting.Domain.Entities.ValueObjects;

namespace Voting.Application.DTOs.Responses
{
    public class VotingSessionResponse
    {
        public uint SessionId { get; set; }
        public Guid AdminUserId { get; set; }
        public RegistrationMode Mode { get; set; }
        public VerificationLevel RequiredVerificationLevel { get; set; }
        public bool VotingActive { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public CandidateDto[] Candidates { get; set; } = [];
        public Guid[] RegisteredVoterIds { get; set; } = [];
    }

    public class CandidateDto
    {
        public uint CandidateId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public uint VoteCount { get; set; }
    }

}
