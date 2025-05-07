using AutoMapper;
using Voting.Application.DTOs.Responses;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Get
{
    /// <summary>
    /// Запрос на получение всей информации по сессии голосования.
    /// </summary>
    public class GetVotingSessionUseCase(IUnitOfWork uow, IMapper mapper)
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<Result<VotingSessionResponse>> Execute(uint sessionId)
        {
            // 1) Загружаем агрегат из репозитория
            var session = await _uow.VotingSessions.GetByIdAsync(sessionId);
            if (session == null)
                return Error.NotFound(
                    "SessionNotFound",
                    $"Voting session with ID {sessionId} was not found.");

            // 2) Картируем агрегат в DTO
            var dto = _mapper.Map<VotingSessionResponse>(session);

            // 3) Возвращаем результат
            return Result<VotingSessionResponse>.Success(dto);
        }
    }
}
