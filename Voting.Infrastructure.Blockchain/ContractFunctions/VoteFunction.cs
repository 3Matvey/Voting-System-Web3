namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("vote")]
    internal class VoteFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
        [Parameter("uint256", "candidateId", 2)] public BigInteger CandidateId { get; set; }
    }
}
