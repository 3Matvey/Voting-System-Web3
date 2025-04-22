namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("VoteCast")]
    internal class VoteCastEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)] public BigInteger SessionId { get; set; }
        [Parameter("address", "voter", 2, true)] public string Voter { get; set; } = string.Empty; //indexed = true для адреса голосующего
        [Parameter("uint256", "candidateId", 3, false)] public BigInteger CandidateId { get; set; }
    }
}
