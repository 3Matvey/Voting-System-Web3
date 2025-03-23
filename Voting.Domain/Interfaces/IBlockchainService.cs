using Voting.Domain.Entities;

namespace Voting.Domain.Interfaces
{
    public interface IBlockchainService
    {
        Task<string> AddCandidateAsync(string candidateNames);
        Task<string> StartVotingAsync(int durationMinutes);
        Task<string> VoteAsync(int candidateId, string voterAddress);
        Task<string> EndVotingAsync();
        Task<IEnumerable<Candidate>> GetResultsAsync();
    }
}
