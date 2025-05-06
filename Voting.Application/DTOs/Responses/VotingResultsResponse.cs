namespace Voting.Application.DTOs.Responses
{
    /// <summary>Результаты сессии голосования.</summary>
    public record VotingResultsResponse(
        uint SessionId,
        List<CandidateResultDto> Candidates
    );
}
