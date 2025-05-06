namespace Voting.Application.DTOs.Responses
{
    public class AddCandidateResponse
    {
        public uint SessionId { get; set; }
        public uint CandidateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
