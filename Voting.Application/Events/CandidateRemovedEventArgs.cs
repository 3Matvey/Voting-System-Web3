namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события удаления кандидата.
    /// </summary>
    public sealed class CandidateRemovedEventArgs(ulong sessionId, ulong candidateId, string name) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public ulong CandidateId { get; } = candidateId;
        public string Name { get; } = name;
    }
}
