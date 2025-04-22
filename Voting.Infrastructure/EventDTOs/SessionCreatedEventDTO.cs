namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("SessionCreated")]
    internal class SessionCreatedEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)] public BigInteger SessionId { get; set; }

        [Parameter("address", "sessionAdmin", 2, false)] public string SessionAdmin { get; set; } = string.Empty;
    }
}
