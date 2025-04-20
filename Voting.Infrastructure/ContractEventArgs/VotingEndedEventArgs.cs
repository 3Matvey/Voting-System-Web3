using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события окончания голосования.
    /// </summary>
    public sealed class VotingEndedEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public long EndTime { get; }

        internal VotingEndedEventArgs(VotingEndedEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            EndTime = (long)dto.EndTime;
        }
    }
}
