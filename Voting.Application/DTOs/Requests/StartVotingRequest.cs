namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на запуск голосования.</summary>
    public record StartVotingRequest(
        int DurationMinutes
    );
}
