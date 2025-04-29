namespace Voting.Domain.Common
{
    /// <summary>
    /// Базовый класс для агрегатных корней, аккумулирует события.
    /// </summary>
    public abstract class AggregateRoot<TKey>
        where TKey : IEquatable<TKey>
    {
        public abstract TKey Id { get; private protected set; } 
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent @event)
            => _domainEvents.Add(@event);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}