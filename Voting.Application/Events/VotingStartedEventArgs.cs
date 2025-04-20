namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события старта голосования.
    /// </summary>
    public sealed class VotingStartedEventArgs(ulong sessionId, DateTime startTimeUtc, DateTime endTimeUtc) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public DateTime StartTimeUtc { get; } = startTimeUtc;
        public DateTime EndTimeUtc { get; } = endTimeUtc;
    }
}
