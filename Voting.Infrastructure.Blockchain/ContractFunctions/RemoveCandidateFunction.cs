namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("removeCandidate")]
    internal class RemoveCandidateFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
        [Parameter("uint256", "candidateId", 2)] public BigInteger CandidateId { get; set; }
    }
}
