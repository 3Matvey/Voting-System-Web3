namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("startVoting")]
    internal class StartVotingFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
        [Parameter("uint256", "durationMinutes", 2)] public BigInteger DurationMinutes { get; set; }
    }
}
