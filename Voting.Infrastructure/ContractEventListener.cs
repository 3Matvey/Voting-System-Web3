using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Voting.Infrastructure.Blockchain.EventDTOs;

namespace Voting.Infrastructure.Blockchain
{
    /// <summary>
    /// Слушатель событий смарт-контракта через WebSocket.
    /// Подписывается на все основные события и вызывает заданные обработчики.
    /// </summary>
    public class ContractEventListener
    {
        private readonly Web3 _web3;
        private readonly string _contractAddress;
        private readonly string _contractAbi;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        /// <summary>
        /// Инициализирует слушатель событий, устанавливая WebSocket-подключение.
        /// </summary>
        /// <param name="wsUrl">URL WebSocket узла.</param>
        /// <param name="contractAddress">Адрес смарт-контракта.</param>
        /// <param name="contractAbi">ABI смарт-контракта.</param>
        public ContractEventListener(string wsUrl, string contractAddress, string contractAbi)
        {
            // Создаем WebSocket клиент для постоянного подключения
            var webSocketClient = new WebSocketClient(wsUrl);
            _web3 = new Web3(webSocketClient);
            _contractAddress = contractAddress;
            _contractAbi = contractAbi;
        }

        /// <summary>
        /// Подписывается на все события смарт-контракта и держит подписки активными до отмены через cancellationToken.
        /// </summary>
        /// <param name="cancellationToken">Токен для отмены подписок.</param>
        public void SubscribeToAllEvents(CancellationToken cancellationToken)
        {
            // Подписываемся на событие SessionCreated
            SubscribeToEvent<SessionCreatedEventDTO>(e =>
            {
                Console.WriteLine($"[SessionCreated] SessionId={e.Event.SessionId}, SessionAdmin={e.Event.SessionAdmin}");
            });

            // Подписываемся на событие CandidateAdded
            SubscribeToEvent<CandidateAddedEventDTO>(e =>
            {
                Console.WriteLine($"[CandidateAdded] SessionId={e.Event.SessionId}, CandidateId={e.Event.CandidateId}, Name={e.Event.Name}");
            });

            // Подписываемся на событие CandidateRemoved
            SubscribeToEvent<CandidateRemovedEventDTO>(e =>
            {
                Console.WriteLine($"[CandidateRemoved] SessionId={e.Event.SessionId}, CandidateId={e.Event.CandidateId}, Name={e.Event.Name}");
            });

            // Подписываемся на событие VotingStarted
            SubscribeToEvent<VotingStartedEventDTO>(e =>
            {
                Console.WriteLine($"[VotingStarted] SessionId={e.Event.SessionId}, StartTime={e.Event.StartTime}, EndTime={e.Event.EndTime}");
            });

            // Подписываемся на событие VotingEnded
            SubscribeToEvent<VotingEndedEventDTO>(e =>
            {
                Console.WriteLine($"[VotingEnded] SessionId={e.Event.SessionId}, EndTime={e.Event.EndTime}");
            });

            // Подписываемся на событие VoteCast
            SubscribeToEvent<VoteCastEventDTO>(e =>
            {
                Console.WriteLine($"[VoteCast] SessionId={e.Event.SessionId}, Voter={e.Event.Voter}, CandidateId={e.Event.CandidateId}");
            });

            // Поддерживаем подписки активными до появления сигнала отмены.
            Task.Run(async () =>
            {
                try
                {
                    // Блокируем поток до отмены через cancellationToken.
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // При отмене освобождаем все подписки.
                    DisposeAllSubscriptions();
                }
            });
        }

        /// <summary>
        /// Универсальный метод подписки на событие заданного типа.
        /// </summary>
        /// <typeparam name="TEventDTO">Тип DTO события, соответствующий смарт-контракту.</typeparam>
        /// <param name="onEvent">Обработчик, вызываемый при получении события.</param>
        private void SubscribeToEvent<TEventDTO>(Action<EventLog<TEventDTO>> onEvent) where TEventDTO : class, new()
        {
            // Получаем обработчик события для заданного типа.
            var eventHandler = _web3.Eth.GetEvent<TEventDTO>(_contractAddress);
            // Создаем фильтр для будущих событий.
            var filterInput = eventHandler.CreateFilterInput();
            // Получаем поток изменений через IObservable.
            var observable = eventHandler.GetAllChangesObservable(filterInput);
            // Подписываемся на события.
            IDisposable subscription = observable.Subscribe(onEvent);
            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Освобождает все активные подписки.
        /// </summary>
        private void DisposeAllSubscriptions()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
