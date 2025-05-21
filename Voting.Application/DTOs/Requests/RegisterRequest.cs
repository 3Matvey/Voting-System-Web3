namespace Voting.Application.DTOs.Requests
{
    public sealed record RegisterRequest(
    string Email,
    string Password,
    string BlockchainAddress);
}
