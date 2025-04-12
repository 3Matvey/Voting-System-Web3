namespace Voting.Application.DTOs
{
    /// <summary>Статус голосования.</summary>
    public record VotingStatusDto(
        bool IsActive,
        uint TimeLeft,
        uint TotalVotesCount
    );
}
