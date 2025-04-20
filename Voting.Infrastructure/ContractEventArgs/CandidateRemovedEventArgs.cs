using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события удаления кандидата.
    /// </summary>
    public sealed class CandidateRemovedEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public ulong CandidateId { get; }
        public string Name { get; }

        internal CandidateRemovedEventArgs(CandidateRemovedEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            CandidateId = (ulong)dto.CandidateId;
            Name = dto.Name;
        }
    }
}
