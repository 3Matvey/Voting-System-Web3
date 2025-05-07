namespace Voting.Application.DTOs.Requests
{
    public class EndVotingRequest
    {
        public Guid AdminUserId { get; set; }
        public uint SessionId { get; set; }
    }

}
