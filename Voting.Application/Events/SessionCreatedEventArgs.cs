namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события создания сессии.
    /// </summary>
    public sealed class SessionCreatedEventArgs(ulong sessionId, string sessionAdmin) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public string SessionAdmin { get; } = sessionAdmin;
    }
}
