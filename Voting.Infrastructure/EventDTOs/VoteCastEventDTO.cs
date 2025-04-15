﻿using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain.EventDTOs
{
    [Event("VoteCast")]
    public class VoteCastEventDTO
    {
        [Parameter("uint256", "sessionId", 1, false)]
        public BigInteger SessionId { get; set; }

        // Указываем indexed = true для адреса голосующего
        [Parameter("address", "voter", 2, true)]
        public string Voter { get; set; }

        [Parameter("uint256", "candidateId", 3, false)]
        public BigInteger CandidateId { get; set; }
    }
}
