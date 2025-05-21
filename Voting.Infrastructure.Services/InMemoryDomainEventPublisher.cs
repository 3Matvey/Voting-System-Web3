using Microsoft.Extensions.Logging;
using Voting.Domain.Common;
using Voting.Domain.Interfaces;

namespace Voting.Infrastructure.Services
{
    public class InMemoryDomainEventPublisher(ILogger logger) : IDomainEventPublisher
    {
        // храним маппинг: тип события → список обработчиков
        private readonly Dictionary<Type, List<Action<IDomainEvent>>> _handlers
            = [];

        // общий лок для доступа к словарю и спискам
        private readonly Lock _lock = new();

        public void Publish(IDomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            List<Action<IDomainEvent>> toInvoke;

            // безопасно копируем список обработчиков под локом
            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var handlers))
                    return;
                toInvoke = [.. handlers];
            }

            // вызываем вне лока и каждый в try/catch
            foreach (var handler in toInvoke)
            {
                try
                {
                    handler(domainEvent);
                }
                catch (Exception ex)
                {

                    logger.LogError(
                        "Error handling domain event {type.Name}: {ex}", type.Name, ex);
                }
            }
        }

        public void Subscribe<T>(Action<T> handler)
            where T : IDomainEvent
        {
            var type = typeof(T);
            // обёртка, приводящая IDomainEvent → T
            void wrapped(IDomainEvent e) => handler((T)e);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var list))
                {
                    list = [];
                    _handlers[type] = list;
                }
                list.Add(wrapped);
            }
        }
    }
}
