using Voting.Application.DTOs.Requests;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    public class StartVotingUseCase(
        ISmartContractAdapter contract,
        IUnitOfWork uow)
    {
        public async Task<Result> Execute(StartVotingRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request cannot be null.");

            var session = await uow.VotingSessions.GetByIdAsync(request.SessionId);
            if (session == null)
                return Error.NotFound("SessionNotFound", $"Session {request.SessionId} not found.");
            if (session.AdminUserId != request.AdminUserId)
                return Error.AccessForbidden("Forbidden", "Only session admin can start voting.");

            try
            {
                await contract.StartVotingAsync(
                    request.SessionId,
                    (uint)request.DurationMinutes);
            }
            catch (Exception ex)
            {
                return Error.Failure("ContractError", ex.Message);
            }

            return Result.Success();
        }

    }
}
