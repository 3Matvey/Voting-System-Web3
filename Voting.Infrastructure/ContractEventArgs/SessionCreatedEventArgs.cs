using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain.ContractEventArgs
{
    /// <summary>
    /// Аргументы события создания сессии.
    /// </summary>
    public sealed class SessionCreatedEventArgs : EventArgs
    {
        public ulong SessionId { get; }
        public string SessionAdmin { get; }

        internal SessionCreatedEventArgs(SessionCreatedEventDTO dto)
        {
            SessionId = (ulong)dto.SessionId;
            SessionAdmin = dto.SessionAdmin;
        }
    }
}
