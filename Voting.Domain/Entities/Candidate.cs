namespace Voting.Domain.Entities
{
    public sealed class Candidate(uint id, string name, uint voteCount)
    {
        public uint Id { get; } = id;
        public string Name { get; set; } = name;
        public uint VoteCount { get; private set; } = voteCount;

        public uint IncrementVote() => VoteCount++;
    }
}
