using System.Reactive.Linq;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain
{
    /// <summary>
    /// Слушатель событий смарт-контракта через WebSocket (Streaming).
    /// Подписывается на все основные события и вызывает заданные обработчики.
    /// </summary>
    /// <remarks>
    /// Инициализирует слушатель событий, устанавливая WebSocket-подключение.
    /// </remarks>
    /// <param name="wsUrl">URL WebSocket-ноды. </param>
    /// <param name="contractAddress">Адрес смарт-контракта.</param>
    public class ContractEventListener(string wsUrl, string contractAddress) : IDisposable
    {
        private readonly StreamingWebSocketClient _streamingWebSocketClient = new(wsUrl);

        /// <summary>
        /// Храним подписки на логи, чтобы потом можно было у них вызвать UnsubscribeAsync().
        /// </summary>
        private readonly List<EthLogsObservableSubscription> _subscriptions = [];

        /// <summary>
        /// Храним IDisposable-объекты, возвращаемые вызовами Subscribe(...).
        /// Они нужны для того, чтобы при остановке освободить ресурсы (Dispose).
        /// </summary>
        private readonly List<IDisposable> _disposableSubscriptions = [];

        private bool _disposed;

        /// <summary>
        /// Запускает WebSocket соединение (подключается к сети).
        /// </summary>
        public async Task StartAsync()
        {
            await _streamingWebSocketClient.StartAsync();
        }

        /// <summary>
        /// Останавливает все подписки и закрывает соединение.
        /// </summary>
        public async Task StopAsync()
        {
            // Сначала отписываемся от всех лог-подписок
            foreach (var subscription in _subscriptions)
            {
                await subscription.UnsubscribeAsync();
            }
            _subscriptions.Clear();

            // Освобождаем все IDisposable, вернувшиеся от .Subscribe(...)
            foreach (var disposable in _disposableSubscriptions)
            {
                disposable.Dispose();
            }
            _disposableSubscriptions.Clear();

            // Останавливаем само WebSocket-соединение
            await _streamingWebSocketClient.StopAsync();
        }

        /// <summary>
        /// Универсальный метод для подписки на событие типа <typeparamref name="T"/>.
        /// Пытается декодировать входящие логи как T и вызывать указанный обработчик.
        /// </summary>
        private async Task SubscribeToEventAsync<T>(Action<T> eventHandler)
            where T : class, IEventDTO, new()
        {
            // Создаём подписку на логи через StreamingWebSocketClient
            var subscription = new EthLogsObservableSubscription(_streamingWebSocketClient);

            // Подписываемся на поток данных Rx
            var disposable = subscription.GetSubscriptionDataResponsesAsObservable()
                .Subscribe(log =>
                {
                    try
                    {
                        // Пытаемся декодировать лог в нужное событие
                        var decoded = Event<T>.DecodeEvent(log);
                        if (decoded != null)
                        {
                            eventHandler(decoded.Event);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при декодировании события {typeof(T).Name}: {ex.Message}");
                    }
                },
                error =>
                {
                    Console.WriteLine($"Ошибка в подписке на событие {typeof(T).Name}: {error.Message}");
                });

            // Сохраняем подписку, чтобы потом вызвать UnsubscribeAsync()
            _subscriptions.Add(subscription);
            // Сохраняем disposable-объект, чтобы потом вызвать Dispose()
            _disposableSubscriptions.Add(disposable);

            // Фильтр логов по адресу контракта (не обрабатываем лишние события)
            var filter = Event<T>.GetEventABI().CreateFilterInput(contractAddress);

            // Запускаем подписку
            await subscription.SubscribeAsync(filter);
        }

        #region Методы для подписки на конкретные события

        public Task SubscribeToSessionCreatedEvent(Action<SessionCreatedEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        public Task SubscribeToCandidateAddedEvent(Action<CandidateAddedEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        public Task SubscribeToCandidateRemovedEvent(Action<CandidateRemovedEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        public Task SubscribeToVotingStartedEvent(Action<VotingStartedEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        public Task SubscribeToVotingEndedEvent(Action<VotingEndedEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        public Task SubscribeToVoteCastEvent(Action<VoteCastEventDTO> handler)
        {
            return SubscribeToEventAsync(handler);
        }

        #endregion

        /// <summary>
        /// Освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ContractEventListener() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopAsync().Wait();
                }
                _disposed = true;
            }
        }
    }
}
