namespace Voting.Application.DTOs
{
    /// <summary>Запрос на создание сессии голосования.</summary>
    public record CreateSessionRequest(
        string SessionAdmin
    );
}
