using AutoMapper;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Create
{
    public class AddCandidateUseCase(IUnitOfWork uof, IMapper mapper)
    {
        public async Task<Result<AddCandidateResponse>> Execute(AddCandidateRequest request)
        {
            //  if (request is null)
            return default; //FIXME
        }
    }
}
