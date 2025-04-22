namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("VotingEnded")]
    internal class VotingEndedEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)] public BigInteger SessionId { get; set; }
        [Parameter("uint256", "endTime", 2, false)] public BigInteger EndTime { get; set; }
    }
}
