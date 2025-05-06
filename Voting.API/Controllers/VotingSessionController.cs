using Microsoft.AspNetCore.Mvc;
using Voting.Application.DTOs.Requests;
using Voting.Application.UseCases.Create;
using Voting.Shared.ResultPattern;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Voting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingSessionsController(
        CreateVotingSessionUseCase create,
        AddCandidateUseCase addCandidate
    ) : BaseController
    {
        public async Task<IActionResult> Create([FromBody] CreateVotingSessionRequest dto)
        {
            var result = await create.Execute(dto);

            return result.Match(
                onSuccess: r => CreatedAtAction(
                nameof(GetById),
                new { id = r.SessionId },
                r),
                onFailure: Problem
            );
        }


        [HttpPost("{sessionId:int}/candidates")]
        public async Task<IActionResult> AddCandidate([FromRoute] uint sessionId, [FromBody] AddCandidateRequest request)
        {
            var result = await addCandidate.Execute(sessionId, request);

            return result.Match(
                onSuccess: Ok,
                onFailure: Problem
            );
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(uint id)
        {
            throw new NotImplementedException();
        }
    }
}
