using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.Events;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    public class AddCandidateUseCase(
        IUnitOfWork uow,
        ISmartContractAdapter contract,
        IContractEventListener listener)
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        [тут атрибут]
        public async Task<Result<AddCandidateResponse>> Execute(uint sessionId, AddCandidateRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request cannot be null.");

            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session is null)
                return Error.NotFound("SessionNotFound", $"Session {sessionId} not found.");

            if (session.AdminUserId != request.AdminUserId)
                return Error.AccessForbidden("Forbidden", "Only session admin can add candidates.");

            // ловит ID через контрактный listener
            var taskCompletionSource = new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(object? _, CandidateAddedEventArgs e)
            {
                if (e.SessionId == sessionId && e.Name == request.Name)
                {
                    taskCompletionSource.TrySetResult(e.CandidateId);
                }
            }
            listener.CandidateAdded += Handler;

            uint candidateId;
            try
            {
                await contract.AddCandidateAsync(sessionId, request.Name);

                using var cts = new CancellationTokenSource(_timeout);
                var completed = await Task.WhenAny(
                    taskCompletionSource.Task,
                    Task.Delay(_timeout, cts.Token)
                );

                if (completed != taskCompletionSource.Task)
                {
                    return Error.Failure("Timeout", "Не получено CandidateAdded-событие за 30 сек.");
                }

                candidateId = await taskCompletionSource.Task;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return Error.Failure("ContractError", ex.Message);
            }
            finally
            {
                listener.CandidateAdded -= Handler;
            }

            return Result<AddCandidateResponse>.Success(new AddCandidateResponse
            {
                SessionId = sessionId,
                CandidateId = candidateId,
                Name = request.Name,
                Description = request.Description
            });
        }
    }
}
