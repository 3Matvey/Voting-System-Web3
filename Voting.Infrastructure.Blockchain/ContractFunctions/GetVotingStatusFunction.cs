namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("getVotingStatus", typeof(VotingStatusOutputDTO))]
    internal class GetVotingStatusFunction : FunctionMessage
    {
        [Parameter("uint256", "sessionId", 1)]
        public BigInteger SessionId { get; set; }
    }

    [FunctionOutput]
    internal class VotingStatusOutputDTO : IFunctionOutputDTO
    {
        [Parameter("bool", "isActive", 1)] public bool IsActive { get; set; }
        [Parameter("uint256", "timeLeft", 2)] public BigInteger TimeLeft { get; set; }
        [Parameter("uint256", "totalVotesCount", 3)] public BigInteger TotalVotesCount { get; set; }
    }
}
