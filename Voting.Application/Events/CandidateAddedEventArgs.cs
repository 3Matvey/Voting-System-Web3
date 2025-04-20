namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события добавления кандидата.
    /// </summary>
    public sealed class CandidateAddedEventArgs(ulong sessionId, ulong candidateId, string name) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public ulong CandidateId { get; } = candidateId;
        public string Name { get; } = name;
    }
}
