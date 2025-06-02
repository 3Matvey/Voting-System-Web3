namespace Voting.Application.DTOs.Responses
{
    /// <summary>Результаты голосования для кандидата.</summary>
    public class CandidateResultDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int VoteCount { get; set; }
    };
}
