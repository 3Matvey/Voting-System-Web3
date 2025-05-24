using Voting.Application.DTOs.Requests;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    public class StartVotingUseCase(
        ISmartContractAdapter contract,
        IUnitOfWork uow)
    {
        public async Task<Result> Execute(uint sessionId, StartVotingRequest request)
        {
            if (request.DurationMinutes == 0)
                return Error.Validation("InvalidRequest", "Voting duration cannot be null.");

            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session == null)
                return Error.NotFound("SessionNotFound", $"Session {sessionId} not found.");
            if (session.AdminUserId != request.AdminUserId)
                return Error.AccessForbidden("Forbidden", "Only session admin can start voting.");

            try
            {
                await contract.StartVotingAsync(
                    sessionId,
                    request.DurationMinutes);
            }
            catch (Exception ex)
            {
                return Error.Failure("ContractError", ex.Message);
            }

            return Result.Success();
        }

    }
}
