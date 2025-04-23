namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("getCandidate", typeof(GetCandidateOutputDTO))]
    internal class GetCandidateFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
        [Parameter("uint256", "candidateId", 2)] public BigInteger CandidateId { get; set; }
    }

    [FunctionOutput]
    internal class GetCandidateOutputDTO : IFunctionOutputDTO
    {
        [Parameter("string", "name", 1)] public string Name { get; set; } = string.Empty;
        [Parameter("uint256", "voteCount", 2)] public BigInteger VoteCount { get; set; }
    }
}
