namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("addCandidate")]
    internal class AddCandidateFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
        [Parameter("string", "name", 2)] public string Name { get; set; } = string.Empty;
    }
}
