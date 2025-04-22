namespace Voting.Application.DTOs
{
    /// <summary>Результаты сессии голосования.</summary>
    public record VotingResultsDto(
        uint SessionId,
        List<CandidateResultDto> Candidates
    );
}
