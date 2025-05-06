namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на добавление кандидата.</summary>
    public class AddCandidateRequest
    {
        public Guid AdminUserId { get; set; }
        public uint SessionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
