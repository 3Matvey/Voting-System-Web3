namespace Voting.Application.DTOs.Responses
{
    /// <summary>Статус голосования.</summary>
    public record VotingStatusResponse(
        bool IsActive,
        uint TimeLeft,
        uint TotalVotesCount
    );
}
