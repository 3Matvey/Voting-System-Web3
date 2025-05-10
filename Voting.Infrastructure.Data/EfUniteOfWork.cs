using Voting.Domain.Interfaces;
using Voting.Domain.Interfaces.Repositories;
using Voting.Infrastructure.Data.Repositories;

namespace Voting.Infrastructure.Data
{
    public class EfUniteOfWork(AppDbContext context) : IUnitOfWork
    {
        private IUserRepository? _userRepository;
        private IVotingSessionRepository? _sessionRepository;

        public IUserRepository Users => _userRepository ??= new UserRepository(context);
        public IVotingSessionRepository VotingSessions => _sessionRepository ??= new VotingSessionRepository(context);

        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
