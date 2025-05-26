using AutoMapper;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Queries.VotingSession
{
    public class GetVotingResultUseCase(IUnitOfWork uow, ISmartContractAdapter contractAdapter, IMapper mapper)
    {
        public async Task<Result<CandidateResultDto[]>> Execute(uint sessionId)
        {
            // Получаем сессию из БД для проверки её существования
            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session is null)
            {
                return Error.NotFound("VotingSession.NotFound", "Сессия голосования не найдена");
            }

            // Получаем статус голосования из смарт-контракта
            var (isActive, _, _) = await contractAdapter.GetVotingStatusAsync(sessionId);
            if (isActive)
            {
                return Error.Validation("VotingSession.Active",
                    "Невозможно получить результаты - голосование всё ещё активно");
            }

            // Получаем результаты из смарт-контракта
            var candidates = await contractAdapter.GetCandidatesAsync(sessionId);

            // Преобразуем в DTO
            var results = mapper.Map<CandidateResultDto[]>(candidates);

            return Result<CandidateResultDto[]>.Success(results);
        }
    }
}