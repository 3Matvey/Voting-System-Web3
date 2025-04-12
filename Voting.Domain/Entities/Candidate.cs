namespace Voting.Domain.Entities
{
    public class Candidate(int id, string name)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public int VoteCount { get; private set; }
        public void IncrementVote() => VoteCount++;
    }
}
