namespace Voting.Application.DTOs
{
    /// <summary>Запрос на добавление кандидата.</summary>
    public record CreateCandidateRequest(
        string CandidateName
    );
}
