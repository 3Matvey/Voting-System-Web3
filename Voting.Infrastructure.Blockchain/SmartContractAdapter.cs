using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Voting.Application.Interfaces;
using Voting.Infrastructure.Blockchain.ContractFunctions;
using Voting.Infrastructure.Blockchain.EventDTOs;
using Nethereum.Contracts.ContractHandlers;
using Voting.Domain.Entities;

namespace Voting.Infrastructure.Blockchain
{
    public class SmartContractAdapter : ISmartContractAdapter
    {
        private readonly ContractHandler _handler;
        private readonly ILogger<SmartContractAdapter>? _logger;
        private readonly string _defaultSenderAddress;
        private readonly Dictionary<uint, string> _sessionAdmins = [];
        private readonly IContractEventListener _listener;

        public SmartContractAdapter(
            string rpcUrl,
            string contractAddress,
            string defaultSenderAddress,
            IContractEventListener listener,
            ILogger<SmartContractAdapter>? logger = null)
        {
            var web3 = new Web3(rpcUrl);
            _handler = web3.Eth.GetContractHandler(contractAddress);
            _defaultSenderAddress = defaultSenderAddress;
            _listener = listener;
            _logger = logger;

            _listener.SessionCreated += (_, e) =>
            {
                _sessionAdmins[e.SessionId] = e.SessionAdmin;
                _logger?.LogDebug(
                    "Mapped session {SessionId} → admin {Admin}",
                    e.SessionId, e.SessionAdmin);
            };
        }

        private string GetSessionAdmin(uint sessionId)
        {
            if (!_sessionAdmins.TryGetValue(sessionId, out var admin))
                throw new InvalidOperationException(
                    $"Session {sessionId} has no known admin");
            return admin;
        }
        public async Task<uint> CreateSessionAsync(string sessionAdmin, CancellationToken ct = default)
        {
            var fn = new CreateSessionFunction { 
                SessionAdmin = sessionAdmin,
                FromAddress = _defaultSenderAddress
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);

            // SessionCreated из receipt
            var evtLog = receipt
                .DecodeAllEvents<SessionCreatedEventDTO>()
                .FirstOrDefault();
            return evtLog != null ? (uint)evtLog.Event.SessionId
                : throw new InvalidOperationException("SessionCreated event not found");
        }

        public async Task<string> AddCandidateAsync(uint sessionId, string candidateName, CancellationToken ct = default)
        {
            var fn = new AddCandidateFunction { 
                SessionId = sessionId, 
                Name = candidateName,
                FromAddress = GetSessionAdmin(sessionId)
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);
            return receipt.TransactionHash;
        }

        public async Task<string> RemoveCandidateAsync(uint sessionId, uint candidateId, CancellationToken ct = default)
        {
            var fn = new RemoveCandidateFunction 
            { 
                SessionId = sessionId,
                CandidateId = candidateId,
                FromAddress = GetSessionAdmin(sessionId)
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);
            return receipt.TransactionHash;
        }

        public async Task<string> StartVotingAsync(uint sessionId, uint durationMinutes, CancellationToken ct = default)
        {
            var fn = new StartVotingFunction 
            { 
                SessionId = sessionId,
                DurationMinutes = durationMinutes,
                FromAddress = GetSessionAdmin(sessionId)
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteAsync(uint sessionId, uint candidateId, User user, CancellationToken ct = default)
        {
            var fn = new VoteFunction 
            { 
                SessionId = sessionId,
                CandidateId = candidateId,
                FromAddress = user.BlockchainAddress, 
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);
            return receipt.TransactionHash;
        }

        public async Task<string> EndVotingAsync(uint sessionId, CancellationToken ct = default)
        {
            var fn = new EndVotingFunction 
            { 
                SessionId = sessionId,
                FromAddress = GetSessionAdmin(sessionId),
            };
            var receipt = await _handler
                .SendRequestAndWaitForReceiptAsync(fn, cancellationToken: ct)
                .ConfigureAwait(false);
            return receipt.TransactionHash;
        }

        public async Task<(bool isActive, uint timeLeft, uint totalVotesCount)> GetVotingStatusAsync(uint sessionId, CancellationToken ct = default)
        {
            var fn = new GetVotingStatusFunction { SessionId = sessionId };

            var dto = await _handler
                .QueryDeserializingToObjectAsync<
                    GetVotingStatusFunction,
                    VotingStatusOutputDTO>(
                    fn,
                    BlockParameter.CreateLatest())
                .ConfigureAwait(false);

            return (
                isActive: dto.IsActive,
                timeLeft: (uint)dto.TimeLeft,
                totalVotesCount: (uint)dto.TotalVotesCount
            );
        }
        public async Task<IEnumerable<Candidate>> GetCandidatesAsync(uint sessionId, CancellationToken ct = default)
        {
            var fn = new GetActiveCandidatesFunction { SessionId = sessionId };
            var dto = await _handler
                .QueryDeserializingToObjectAsync<
                    GetActiveCandidatesFunction,
                    GetActiveCandidatesOutputDTO>(
                    fn,
                    BlockParameter.CreateLatest())
                .ConfigureAwait(false);

            return [.. dto.Ids
                .Select((id, i) => new Candidate(
                    (uint)id,
                    dto.Names[i],
                    (uint)dto.VoteCounts[i]
                ))];
        }

        public async Task<Candidate> GetCandidateAsync(uint sessionId, uint candidateId, CancellationToken ct = default)
        {
            var fn = new GetCandidateFunction
            {
                SessionId = sessionId,
                CandidateId = candidateId
            };
            var dto = await _handler
                .QueryDeserializingToObjectAsync<
                    GetCandidateFunction,
                    GetCandidateOutputDTO>(
                    fn,
                    BlockParameter.CreateLatest())
                .ConfigureAwait(false);

            return new Candidate(
                candidateId,
                dto.Name,
                (uint)dto.VoteCount
            );
        }
    }
}
