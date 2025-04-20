namespace Voting.Application.Events
{
    /// <summary>
    /// Аргументы события голосования за кандидата.
    /// </summary>
    public sealed class VoteCastEventArgs(ulong sessionId, string voter, ulong candidateId) : EventArgs
    {
        public ulong SessionId { get; } = sessionId;
        public string Voter { get; } = voter;
        public ulong CandidateId { get; } = candidateId;

    }
}
