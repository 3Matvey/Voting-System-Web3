namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на запуск голосования.</summary>
    public class StartVotingRequest
    {
        public Guid AdminUserId { get; set; }
        public uint DurationMinutes { get; set; }
    }
}
