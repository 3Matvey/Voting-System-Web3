namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("getActiveCandidates", typeof(GetActiveCandidatesOutputDTO))]
    internal class GetActiveCandidatesFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)] public BigInteger SessionId { get; set; }
    }

    [FunctionOutput]
    internal class GetActiveCandidatesOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "ids", 1)] public List<BigInteger> Ids { get; set; } = [];
        [Parameter("string[]", "names", 2)] public List<string> Names { get; set; } = [];
        [Parameter("uint256[]", "voteCounts", 3)] public List<BigInteger> VoteCounts { get; set; } = [];
    }
}
