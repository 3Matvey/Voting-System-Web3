namespace Voting.Application.DTOs.Requests
{
    public sealed record LoginRequest(
    string Email,
    string Password);
}
