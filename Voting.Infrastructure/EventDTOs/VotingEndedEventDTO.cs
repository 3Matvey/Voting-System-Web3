using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("VotingEnded")]
    public class VotingEndedEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)]
        public BigInteger SessionId { get; set; }

        [Parameter("uint256", "endTime", 2, false)]
        public BigInteger EndTime { get; set; }
    }
}
