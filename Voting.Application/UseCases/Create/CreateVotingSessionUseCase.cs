using AutoMapper;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Create
{
    public class CreateVotingSessionUseCase(IUnitOfWork uow, ISmartContractAdapter contract, IMapper mapper)
    {   
        public async Task<Result<CreateVotingSessionResponse>> Execute(CreateVotingSessionRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request must not be null.");

            if (request.AdminUserId == Guid.Empty)
                return Error.Validation("InvalidAdminId", "AdminUserId must be provided.");

            var admin = await uow.Users.GetByIdAsync(request.AdminUserId);
            if (admin == null)
                return Error.NotFound("AdminNotFound", $"No user found with ID {request.AdminUserId}.");

            uint sessionId;
            try
            {
                sessionId = await contract.CreateSessionAsync(admin.BlockchainAddress);
            }
            catch (Exception ex)
            {
                return Error.Failure("ContractError", $"Failed to create on-chain session: {ex.Message}");
            }

            // 3) Инициализируем агрегат с полным off-chain состоянием
            var session = new VotingSessionAggregate();
            session.Apply(new SessionCreatedDomainEvent(
                sessionId,
                admin.Id,
                request.Mode,
                request.RequiredVerificationLevel
            ));

            // 4) Сохраняем агрегат и коммитим (вместе с DomainEvents)
            await uow.VotingSessions.AddAsync(session);
            await uow.CommitAsync();

            // 5) Формируем ответ
            var response = mapper.Map<CreateVotingSessionResponse>(session);
            return Result<CreateVotingSessionResponse>.Success(response);
        }
    }
}
