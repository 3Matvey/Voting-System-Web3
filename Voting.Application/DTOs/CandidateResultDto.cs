namespace Voting.Application.DTOs
{
    /// <summary>Результаты голосования для кандидата.</summary>
    public record CandidateResultDto(
        int CandidateId,
        string CandidateName,
        int VoteCount
    );
}
