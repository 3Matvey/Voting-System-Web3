using Voting.Domain.Common;

namespace Voting.Domain.Interfaces
{
    public interface IDomainEventPublisher
    {
        void Publish(IDomainEvent domainEvent);
        void Subscribe<T>(Action<T> handler)
            where T : IDomainEvent;
    }
}
