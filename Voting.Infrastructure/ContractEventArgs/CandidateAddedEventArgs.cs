using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события добавления кандидата.
    /// </summary>
    public sealed class CandidateAddedEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public ulong CandidateId { get; }
        public string Name { get; }

        internal CandidateAddedEventArgs(CandidateAddedEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            CandidateId = (ulong)dto.CandidateId;
            Name = dto.Name;
        }
    }
}
