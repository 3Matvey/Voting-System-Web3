namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события создания сессии.
    /// </summary>
    public sealed class SessionCreatedEventArgs(uint sessionId, string sessionAdmin) : EventArgs
    {
        public uint SessionId { get; } = sessionId;
        public string SessionAdmin { get; } = sessionAdmin;
    }
}
