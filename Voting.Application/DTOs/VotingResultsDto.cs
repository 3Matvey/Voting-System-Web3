namespace Voting.Application.DTOs
{
    /// <summary>Результаты сессии голосования.</summary>
    public record VotingResultsDto(
        Guid SessionId,
        List<CandidateResultDto> Candidates
    );
}
