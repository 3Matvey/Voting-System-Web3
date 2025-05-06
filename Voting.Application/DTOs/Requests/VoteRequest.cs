namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на голосование.</summary>
    public record VoteRequest(
        Guid VoterId,
        int CandidateId
    );
}
