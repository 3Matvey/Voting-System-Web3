namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события добавления кандидата.
    /// </summary>
    public sealed class CandidateAddedEventArgs(uint sessionId, uint candidateId, string name) : EventArgs
    {
        public uint SessionId { get; } = sessionId;
        public uint CandidateId { get; } = candidateId;
        public string Name { get; } = name;
    }
}
