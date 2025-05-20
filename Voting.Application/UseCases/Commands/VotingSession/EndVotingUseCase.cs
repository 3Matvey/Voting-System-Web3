using Voting.Application.DTOs.Requests;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    /// <summary>
    /// Завершает голосование on-chain.
    /// </summary>
    public class EndVotingUseCase(
        ISmartContractAdapter contract,
        IUnitOfWork uow)
    {
        private readonly ISmartContractAdapter _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));

        public async Task<Result> Execute(EndVotingRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request cannot be null.");

            var session = await _uow.VotingSessions.GetByIdAsync(request.SessionId);
            if (session == null)
                return Error.NotFound("SessionNotFound", $"Session {request.SessionId} not found.");
            if (session.AdminUserId != request.AdminUserId)
                return Error.AccessForbidden("Forbidden", "Only session admin can end voting.");

            try
            {
                await _contract.EndVotingAsync(request.SessionId);
            }
            catch (Exception ex)
            {
                return Error.Failure("ContractError", ex.Message);
            }

            return Result.Success();
        }
    }
}
