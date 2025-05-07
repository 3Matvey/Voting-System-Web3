using System;
using System.Threading.Tasks;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Events;
using Voting.Domain.Interfaces;
using Voting.Shared.ResultPattern;

namespace Voting.Application.UseCases.Create
{
    public class CreateVotingSessionUseCase(
        IUnitOfWork uow,
        ISmartContractAdapter contract,
        IDomainEventPublisher publisher)
    {
        private readonly ISmartContractAdapter _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        private readonly IDomainEventPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

        public async Task<Result<CreateVotingSessionResponse>> Execute(
            CreateVotingSessionRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request must not be null.");
            if (request.AdminUserId == Guid.Empty)
                return Error.Validation("InvalidAdminId", "AdminUserId must be provided.");

            var admin = await uow.Users.GetByIdAsync(request.AdminUserId);
            if (admin == null)
                return Error.NotFound("UserNotFound", $"User with {request.AdminUserId} not found");

            uint sessionId;
            try
            {
                sessionId = await _contract.CreateSessionAsync(admin.BlockchainAddress);
            }
            catch (Exception ex)
            {
                return Error.Failure("ContractError",
                    $"Failed to create on-chain session: {ex.Message}");
            }

            // публикуем off-chain доменное событие — проекция его подхватит и сохранит
            _publisher.Publish(new SessionCreatedDomainEvent(
                sessionId,
                request.AdminUserId,
                request.Mode,
                request.RequiredVerificationLevel
            ));

            // готовим ответ
            var response = new CreateVotingSessionResponse
            {
                SessionId = sessionId,
                AdminUserId = request.AdminUserId,
                Mode = request.Mode,
                RequiredVerificationLevel = request.RequiredVerificationLevel
            };

            return Result<CreateVotingSessionResponse>.Success(response);
        }
    }
}
