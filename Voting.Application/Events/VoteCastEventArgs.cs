namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события голосования за кандидата.
    /// </summary>
    public sealed class VoteCastEventArgs(uint sessionId, string voter, uint candidateId) : EventArgs
    {
        public uint SessionId { get; } = sessionId;
        public string Voter { get; } = voter;
        public uint CandidateId { get; } = candidateId;

    }
}
