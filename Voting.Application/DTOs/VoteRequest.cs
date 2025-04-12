namespace Voting.Application.DTOs
{
    /// <summary>Запрос на голосование.</summary>
    public record VoteRequest(
        Guid VoterId,
        int CandidateId
    );
}
