using AutoMapper;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Queries.VotingSession
{
    //FIXME постоянные запросы к блокчейну неоптимальны, желательно единоразово сохранять в БД
    public class GetVotingResultUseCase(IUnitOfWork uow, ISmartContractAdapter contractAdapter, IMapper mapper)
    {
        public async Task<Result<CandidateResultDto[]>> Execute(uint sessionId)
        {
            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session is null)
            {
                return Error.NotFound("VotingSession.NotFound", "Сессия голосования не найдена");
            }

            var (isActive, _, _) = await contractAdapter.GetVotingStatusAsync(sessionId);
            if (isActive)
            {
                return Error.Validation("VotingSession.Active",
                    "Невозможно получить результаты - голосование всё ещё активно");
            }

            var candidates = await contractAdapter.GetCandidatesAsync(sessionId);

            var results = mapper.Map<CandidateResultDto[]>(candidates);

            return Result<CandidateResultDto[]>.Success(results);
        }
    }
}