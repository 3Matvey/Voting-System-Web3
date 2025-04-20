using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события голосования за кандидата.
    /// </summary>
    public sealed class VoteCastEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public string Voter { get; }
        public ulong CandidateId { get; }

        internal VoteCastEventArgs(VoteCastEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            Voter = dto.Voter;
            CandidateId = (ulong)dto.CandidateId;
        }
    }
}
