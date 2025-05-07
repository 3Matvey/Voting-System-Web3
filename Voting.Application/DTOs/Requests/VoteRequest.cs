namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на голосование.</summary>
    public class VoteRequest
    {
        public Guid UserId { get; set; }
        public uint SessionId { get; set; }
        public uint CandidateId { get; set; }
    }
}
