namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("endVoting")]
    internal class EndVotingFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] 
        public BigInteger SessionId { get; set; }
    }
}
