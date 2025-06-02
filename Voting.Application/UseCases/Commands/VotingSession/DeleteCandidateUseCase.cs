using Voting.Application.DTOs.Requests;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;  // <- для ISmartContractAdapter

namespace Voting.Application.UseCases.Commands.VotingSession
{
    /// <summary>
    /// Use Case для удаления (деактивации) кандидата из сессии голосования.
    /// </summary>
    public class DeleteCandidateUseCase(
        IUnitOfWork uow,
        ISmartContractAdapter contractAdapter
        )
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly ISmartContractAdapter _contractAdapter = contractAdapter ?? throw new ArgumentNullException(nameof(contractAdapter));

        /// <summary>
        /// Удаляет кандидата из сессии голосования.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии голосования (on‐chain).</param>
        /// <param name="request">DTO с полями AdminUserId и CandidateId.</param>
        /// <returns>Result.Success() при успешном выполнении, иначе — Error.</returns>
        public async Task<Result> Execute(uint sessionId, DeleteCandidateRequest request)
        {
            if (request is null || sessionId == 0)
            {
                return Error.Validation(
                    "InvalidRequest",
                    "Request cannot be null and sessionId must be greater than zero."
                );
            }

            var session = await _uow.VotingSessions.GetByIdAsync(sessionId);
            if (session is null)
            {
                return Error.NotFound(
                    "VotingSession.NotFound",
                    $"Сессия голосования с id = {sessionId} не найдена."
                );
            }

            if (session.AdminUserId != request.AdminUserId)
            {
                return Error.AccessForbidden(
                    "VotingSession.Forbidden",
                    "Только администратор сессии может удалять кандидатов."
                );
            }

            if (session.VotingActive) 
            {
                return Error.Validation(
                    "VotingSession.AlreadyStarted",
                    "Нельзя удалять кандидатов после того, как голосование уже началось."
                );
            }

            var candidate = session.Candidates.FirstOrDefault(c => c.Id == request.CandidateId);
            if (candidate is null)
            {
                return Error.NotFound(
                    "Candidate.NotFound",
                    $"Кандидат с id = {request.CandidateId} не найден в сессии {sessionId}."
                );
            }

            try
            {
                await _contractAdapter.RemoveCandidateAsync(sessionId, request.CandidateId);
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "ContractError",
                    $"Не удалось вызвать контракт для удаления кандидата: {ex.Message}"
                );
            }

            return Result.Success();
        }
    }
}
