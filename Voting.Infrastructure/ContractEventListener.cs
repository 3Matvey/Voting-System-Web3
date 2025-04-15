using System.Reactive.Linq;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Voting.Infrastructure.Blockchain.EventDTOs;
using Microsoft.Extensions.Logging;

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
    public class ContractEventListener(string wsUrl, string contractAddress, ILogger<ContractEventListener>? logger = default) : IDisposable
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
            logger?.LogInformation("WebSocket соединение запущено");
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
            logger?.LogInformation("WebSocket соединение остановлено");
        }

        /// <summary>
        /// Приватный универсальный метод подписки на событие конкретного DTO.
        /// При успешном декодировании события вызывается переданный параметрless‑callback.
        /// </summary>
        /// <typeparam name="T">Тип DTO события (например, SessionCreatedEventDTO),
        /// который должен реализовывать IEventDTO и иметь конструктор по умолчанию.</typeparam>
        /// <param name="callback">Параметрless‑callback, вызываемый при получении события.</param>
        private async Task SubscribeToEventInternal<T>(Action callback)
            where T : class, IEventDTO, new()
        {
            var subscription = new EthLogsObservableSubscription(_streamingWebSocketClient);

            var disposable = subscription.GetSubscriptionDataResponsesAsObservable()
                .Subscribe(log =>
                {
                    try
                    {
                        var decoded = Event<T>.DecodeEvent(log);
                        if (decoded != null)
                        {
                            // Не передаем DTO, просто уведомляем об успешном получении события.
                            callback();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Ошибка при декодировании события {EventName}", typeof(T).Name);
                    }
                },
                error =>
                {
                    logger?.LogError(error, "Ошибка в подписке на событие {EventName}", typeof(T).Name);
                });

            _subscriptions.Add(subscription);
            _disposableSubscriptions.Add(disposable);

            // Фильтруем только те логи, которые исходят от нужного контракта
            var filter = Event<T>.GetEventABI().CreateFilterInput(contractAddress);
            await subscription.SubscribeAsync(filter);
        }

        #region Методы подписки на конкретные события

        public event Action SessionCreatedEvent
        {
            add => SubscribeToEventInternal<SessionCreatedEventDTO>(value);
            remove => throw new NotImplementedException();
        }
        async Task smth()
        {
            ContractEventListener a = new("s", "s");
            a.SessionCreatedEvent += async () =>
            await s();

            async Task s()
            {
                await Task.Delay(1);
            }
        }
        /// <summary>
        /// Подписка на событие SessionCreated.
        /// </summary>
        public Task SubscribeToSessionCreatedEvent(Action callback)
        {
            return SubscribeToEventInternal<SessionCreatedEventDTO>(callback);
        }

        /// <summary>
        /// Подписка на событие CandidateAdded.
        /// </summary>
        public Task SubscribeToCandidateAddedEvent(Action callback)
        {
            return SubscribeToEventInternal<CandidateAddedEventDTO>(callback);
        }

        /// <summary>
        /// Подписка на событие CandidateRemoved.
        /// </summary>
        public Task SubscribeToCandidateRemovedEvent(Action callback)
        {
            return SubscribeToEventInternal<CandidateRemovedEventDTO>(callback);
        }

        /// <summary>
        /// Подписка на событие VotingStarted.
        /// </summary>
        public Task SubscribeToVotingStartedEvent(Action callback)
        {
            return SubscribeToEventInternal<VotingStartedEventDTO>(callback);
        }

        /// <summary>
        /// Подписка на событие VotingEnded.
        /// </summary>
        public Task SubscribeToVotingEndedEvent(Action callback)
        {
            return SubscribeToEventInternal<VotingEndedEventDTO>(callback);
        }
        
        /// <summary>
        /// Подписка на событие VoteCast.
        /// </summary>
        public Task SubscribeToVoteCastEvent(Action callback)
        {
            return SubscribeToEventInternal<VoteCastEventDTO>(callback);
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
