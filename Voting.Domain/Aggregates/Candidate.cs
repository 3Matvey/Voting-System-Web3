namespace Voting.Domain.Aggregates
{
    public sealed class Candidate(uint id, string name, uint voteCount)
    {
        public uint Id { get; } = id;
        public string Name { get; } = name;
        public uint VoteCount { get; private set; } = voteCount;
        internal void IncrementVote() => VoteCount++;
    }
}
