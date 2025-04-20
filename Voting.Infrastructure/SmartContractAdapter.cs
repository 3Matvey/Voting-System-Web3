using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Voting.Application.Interfaces;
using Voting.Domain.Entities;

namespace Voting.Infrastructure.Blockchain
{
    /// <summary>
    /// Адаптер для взаимодействия со смарт-контрактом голосования через Nethereum.
    /// Реализует интерфейс ISmartContractAdapter.
    /// </summary>
    public class SmartContractAdapter(string rpcUrl, string contractAddress, string contractAbi, string defaultSenderAddress) : ISmartContractAdapter
    {
        private readonly Web3 _web3 = new Web3(rpcUrl);

        public async Task<uint> CreateSessionAsync(string sessionAdmin)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var createSessionFunction = contract.GetFunction("createSession");
            // Отправляем транзакцию от _defaultSenderAddress (суперадмина) для создания сессии.
            var gas = new HexBigInteger(3000000);
            TransactionReceipt receipt = await createSessionFunction.SendTransactionAndWaitForReceiptAsync(
                defaultSenderAddress, gas, null, null, sessionAdmin);

            // Получение сессионного идентификатора (например, через событие или возвращаемое значение).
            // Здесь возвращается placeholder (0) – в реальной реализации следует извлечь sessionId.
            return 0;
        }

        public async Task<string> AddCandidateAsync(uint sessionId, string candidateName)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var addCandidateFunction = contract.GetFunction("addCandidate");
            var gas = new HexBigInteger(3000000);
            // Предполагается, что вызов выполняется от адреса администратора сессии.
            TransactionReceipt receipt = await addCandidateFunction.SendTransactionAndWaitForReceiptAsync(
                defaultSenderAddress, gas, null, null, sessionId, candidateName);
            return receipt.TransactionHash;
        }

        public async Task<string> RemoveCandidateAsync(uint sessionId, uint candidateId)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var removeCandidateFunction = contract.GetFunction("removeCandidate");
            var gas = new HexBigInteger(3000000);
            TransactionReceipt receipt = await removeCandidateFunction.SendTransactionAndWaitForReceiptAsync(
                defaultSenderAddress, gas, null, null, sessionId, candidateId);
            return receipt.TransactionHash;
        }

        public async Task<string> StartVotingAsync(uint sessionId, uint durationMinutes)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var startVotingFunction = contract.GetFunction("startVoting");
            var gas = new HexBigInteger(3000000);
            TransactionReceipt receipt = await startVotingFunction.SendTransactionAndWaitForReceiptAsync(
                defaultSenderAddress, gas, null, null, sessionId, durationMinutes);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteAsync(uint sessionId, uint candidateId, string voterAddress)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var voteFunction = contract.GetFunction("vote");
            var gas = new HexBigInteger(3000000);
            // Отправляем транзакцию от адреса голосующего.
            TransactionReceipt receipt = await voteFunction.SendTransactionAndWaitForReceiptAsync(
                voterAddress, gas, null, null, sessionId, candidateId);
            return receipt.TransactionHash;
        }

        public async Task<string> EndVotingAsync(uint sessionId)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var endVotingFunction = contract.GetFunction("endVoting");
            var gas = new HexBigInteger(3000000);
            TransactionReceipt receipt = await endVotingFunction.SendTransactionAndWaitForReceiptAsync(
                defaultSenderAddress, gas, null, null, sessionId);
            return receipt.TransactionHash;
        }

        public async Task<(bool isActive, uint timeLeft, uint totalVotesCount)> GetVotingStatusAsync(uint sessionId)
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var getVotingStatusFunction = contract.GetFunction("getVotingStatus");
            // Вызов запроса (без отправки транзакции) для получения данных.
            var result = await getVotingStatusFunction.CallDeserializingToObjectAsync<VotingStatusDTO>(sessionId);
            return (result.IsActive, result.TimeLeft, result.TotalVotesCount);
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesAsync(uint sessionId)
        {
            // Если в контракте нет функции для получения списка кандидатов напрямую,
            // можно реализовать получение списка через события или отдельный запрос.
            // Здесь возвращается пустой список в качестве заглушки.
            return new List<Candidate>();
        }
    }

    /// <summary>
    /// DTO для десериализации статуса голосования, возвращаемого смарт-контрактом.
    /// Именование полей должно соответствовать возвращаемым данным.
    /// </summary>
    public class VotingStatusDTO
    {
        public bool IsActive { get; set; }
        public uint TimeLeft { get; set; }
        public uint TotalVotesCount { get; set; }
    }
}
