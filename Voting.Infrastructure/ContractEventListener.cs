using Microsoft.Extensions.Logging;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Contracts;
using Voting.Infrastructure.Blockchain.EventDTOs;
using Voting.Infrastructure.Blockchain.ContractEventArgs;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Voting.Infrastructure.Blockchain
{
    /// <summary>
    /// Слушатель событий смарт-контракта через WebSocket (Streaming).
    /// Подписывается на все основные события и вызывает заданные обработчики.
    /// </summary>
    public class ContractEventListener(string wsUrl, string contractAddress, ILogger<ContractEventListener>? logger = null)
        : IAsyncDisposable
    {
        private readonly StreamingWebSocketClient _client = new(wsUrl);
        private readonly string _contractAddress = contractAddress;
        private readonly ILogger<ContractEventListener>? _logger = logger;
        private readonly List<EthLogsObservableSubscription> _subscriptions = new();
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        public event EventHandler<SessionCreatedEventArgs>? SessionCreated;
        public event EventHandler<CandidateAddedEventArgs>? CandidateAdded;
        public event EventHandler<CandidateRemovedEventArgs>? CandidateRemoved;
        public event EventHandler<VotingStartedEventArgs>? VotingStarted;
        public event EventHandler<VotingEndedEventArgs>? VotingEnded;
        public event EventHandler<VoteCastEventArgs>? VoteCast;

        public async Task StartAsync()
        {
            await _client.StartAsync();
            _logger?.LogInformation("WebSocket connection started to {Url}", wsUrl);
            await InitializeSubscriptionsAsync();
        }

        public async Task StopAsync()
        {
            foreach (var sub in _subscriptions)
                await sub.UnsubscribeAsync();
            _subscriptions.Clear();

            foreach (var disp in _disposables)
                disp.Dispose();
            _disposables.Clear();

            await _client.StopAsync();
            _logger?.LogInformation("WebSocket connection stopped");
        }

        private async Task InitializeSubscriptionsAsync()
        {
            await Subscribe<SessionCreatedEventDTO, SessionCreatedEventArgs>(
                dto => new SessionCreatedEventArgs(dto),
                (s, e) => SessionCreated?.Invoke(s, e));

            await Subscribe<CandidateAddedEventDTO, CandidateAddedEventArgs>(
                dto => new CandidateAddedEventArgs(dto),
                (s, e) => CandidateAdded?.Invoke(s, e));

            await Subscribe<CandidateRemovedEventDTO, CandidateRemovedEventArgs>(
                dto => new CandidateRemovedEventArgs(dto),
                (s, e) => CandidateRemoved?.Invoke(s, e));

            await Subscribe<VotingStartedEventDTO, VotingStartedEventArgs>(
                dto => new VotingStartedEventArgs(dto),
                (s, e) => VotingStarted?.Invoke(s, e));

            await Subscribe<VotingEndedEventDTO, VotingEndedEventArgs>(
                dto => new VotingEndedEventArgs(dto),
                (s, e) => VotingEnded?.Invoke(s, e));

            await Subscribe<VoteCastEventDTO, VoteCastEventArgs>(
                dto => new VoteCastEventArgs(dto),
                (s, e) => VoteCast?.Invoke(s, e));
        }

        private async Task Subscribe<TDto, TArgs>(Func<TDto, TArgs> map, Action<object, TArgs> raise)
            where TDto : class, IEventDTO, new()
            where TArgs : EventArgs
        {
            var subscription = new EthLogsObservableSubscription(_client);
            var disposable = subscription.GetSubscriptionDataResponsesAsObservable()
                .Subscribe(log =>
                {
                    var decoded = Event<TDto>.DecodeEvent(log);
                    if (decoded != null)
                    {
                        var args = map(decoded.Event);
                        raise(this, args);
                    }
                },
                error => _logger?.LogError(error, "Error in subscription for {Event}", typeof(TDto).Name));

            _subscriptions.Add(subscription);
            _disposables.Add(disposable);

            var filter = Event<TDto>.GetEventABI().CreateFilterInput(_contractAddress);
            await subscription.SubscribeAsync(filter);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            await StopAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
