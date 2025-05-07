using Voting.Domain.Entities.ValueObjects;

namespace Voting.Application.DTOs.Responses
{
    public class CreateVotingSessionResponse
    {
        public uint SessionId { get; set; }
        public Guid AdminUserId { get; set; }
        public RegistrationMode Mode { get; set; }
        public VerificationLevel RequiredVerificationLevel { get; set; }
    }
}
