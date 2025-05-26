namespace Voting.Application.DTOs.Requests
{
    public class CastVoteRequest
    {
        public Guid UserId { get; set; }
        public uint CandidateId { get; set; }
    }
}
