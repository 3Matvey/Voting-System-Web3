using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("SessionCreated")]
    public class SessionCreatedEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)]
        public BigInteger SessionId { get; set; }

        [Parameter("address", "sessionAdmin", 2, false)]
        public string SessionAdmin { get; set; }
    }
}
