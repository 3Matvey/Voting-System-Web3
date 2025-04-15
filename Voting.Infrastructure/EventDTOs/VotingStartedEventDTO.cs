using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("VotingStarted")]
    internal class VotingStartedEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)]
        public BigInteger SessionId { get; set; }

        [Parameter("uint256", "startTime", 2, false)]
        public BigInteger StartTime { get; set; }

        [Parameter("uint256", "endTime", 3, false)]
        public BigInteger EndTime { get; set; }
    }
}
