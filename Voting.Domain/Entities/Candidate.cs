namespace Voting.Domain.Entities
{
    public class Candidate(int id, string name)
    {
        private int _voteCount = 0;
        public int Id { get; private set; } = id;
        public string Name { get; private set; } = name;
        public int VoteCount => _voteCount;

        public void IncrementVote()
        {
            _voteCount++;
        }
    }
}
