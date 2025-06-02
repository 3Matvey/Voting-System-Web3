namespace Voting.Application.DTOs.Requests
{
    public class DeleteCandidateRequest
    {
        public Guid AdminUserId { get; set; }
        public uint CandidateId { get; set; }
    }
}
