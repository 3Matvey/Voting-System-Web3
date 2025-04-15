using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("CandidateAdded")]
    public class CandidateAddedEventDTO : IEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)]
        public BigInteger SessionId { get; set; }

        [Parameter("uint256", "candidateId", 2, false)]
        public BigInteger CandidateId { get; set; }

        [Parameter("string", "name", 3, false)]
        public string Name { get; set; }
    }
}
