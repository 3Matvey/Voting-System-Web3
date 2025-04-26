namespace Voting.Domain.Entities
{
    public sealed class Candidate(uint id, string name, uint voteCount = 0)
    {
        public uint Id { get; } = id;
        public string Name { get; } = name;
        public string Description { get; private set; } = string.Empty;
        public uint VoteCount { get; private set; } = voteCount;
        internal void IncrementVote() => VoteCount++;
        internal void UpdateDescription(string newDescr) => Description = newDescr;
    }
}
