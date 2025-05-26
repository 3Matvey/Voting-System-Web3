using Voting.Application.DTOs.Requests;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    public class CastVoteUseCase(ISmartContractAdapter contract, IUnitOfWork uof)
    {
        public async Task<Result> Execute(uint sessionId, CastVoteRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request cannot be null.");

            var user = await uof.Users.GetByIdAsync(request.UserId);

            if (user == null)
                return Error.NotFound("UserNotFound", $"Session {request.UserId} not found");
             
            try
            {
                await contract.VoteAsync(sessionId, request.CandidateId, user);
            }
            catch (Exception ex) 
            {
                return Error.Failure("ContractError", ex.Message);
            }

            return Result.Success();
        }
    }
}
