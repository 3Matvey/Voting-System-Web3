namespace Voting.Application.DTOs
{
    /// <summary>Запрос на запуск голосования.</summary>
    public record StartVotingRequest(
        int DurationMinutes
    );
}
