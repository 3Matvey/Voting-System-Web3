namespace Voting.Application.DTOs.Responses
{
    /// <summary>Результаты голосования для кандидата.</summary>
    public record CandidateResultDto(
        int CandidateId,
        string CandidateName,
        int VoteCount
    );
}
