
namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события окончания голосования.
    /// </summary>
    public sealed class VotingEndedEventArgs(ulong sessionId, DateTime endTimeUtc) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public DateTime EndTimeUtc { get; } = endTimeUtc;
    }
}
