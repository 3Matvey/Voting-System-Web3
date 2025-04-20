using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события старта голосования.
    /// </summary>
    public sealed class VotingStartedEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public long StartTime { get; }
        public long EndTime { get; }

        internal VotingStartedEventArgs(VotingStartedEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            StartTime = (long)dto.StartTime;
            EndTime = (long)dto.EndTime;
        }
    }
}
