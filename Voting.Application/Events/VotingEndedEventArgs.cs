
namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события окончания голосования.
    /// </summary>
    public sealed class VotingEndedEventArgs(uint sessionId, DateTime endTimeUtc) : EventArgs
    {
        public uint SessionId { get; } = sessionId;
        public DateTime EndTimeUtc { get; } = endTimeUtc;
    }
}
