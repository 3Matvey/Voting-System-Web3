namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события старта голосования.
    /// </summary>
    public sealed class VotingStartedEventArgs(uint sessionId, DateTime startTimeUtc, DateTime endTimeUtc) : EventArgs
    {
        public uint SessionId { get; } = sessionId;
        public DateTime StartTimeUtc { get; } = startTimeUtc;
        public DateTime EndTimeUtc { get; } = endTimeUtc;
    }
}
