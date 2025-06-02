using Microsoft.AspNetCore.Mvc;
using Voting.Application.DTOs.Requests;
using Voting.Application.UseCases.Commands.VotingSession;
using Voting.Application.UseCases.Queries.VotingSession;
using Voting.Shared.ResultPattern;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Voting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingSessionsController(
        CreateVotingSessionUseCase create,
        AddCandidateUseCase addCandidate,
        GetVotingSessionUseCase getVotingSession,
        StartVotingUseCase startVoting,
        EndVotingUseCase endVoting,
        CastVoteUseCase castVote,
        GetVotingResultUseCase getVotingResult,
        DeleteCandidateUseCase deleteCandidate
    ) : ControllerBaseWithResult
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVotingSessionRequest dto)
        {
            var result = await create.Execute(dto);

            return result.Match(
                onSuccess: r =>
                {
                    var body = new
                    {
                        Message = "Voting session created successfully",
                        Session = r
                    };

                    return CreatedAtRoute(
                        //routeName: nameof(GetById),
                        routeValues: new { sessionId = r.SessionId },
                        value: body);
                },
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

        [HttpPost("{sessionId:int}/start")]
        public async Task<IActionResult> StartVoting([FromRoute] uint sessionId, [FromBody] StartVotingRequest request)
        {
            var result = await startVoting.Execute(sessionId, request);

            return result.Match(
                onSuccess: Ok,
                onFailure: Problem
            );
        }

        [HttpPost("{sessionId:int}/end")]
        public async Task<IActionResult> EndVoting([FromRoute] uint sessionId, [FromBody] EndVotingRequest request)
        {
            var result = await endVoting.Execute(sessionId, request);

            return result.Match(
                onSuccess: Ok,
                onFailure: Problem
            );
        }

        [HttpPost("{sessionId:int}/vote")]
        public async Task<IActionResult> CastVote([FromRoute] uint sessionId, [FromBody] CastVoteRequest request)
        {
            var result = await castVote.Execute(sessionId, request);

            return result.Match(
                onSuccess: Ok,
                onFailure: Problem
            );
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] uint id)
        {
            var result = await getVotingSession.Execute(id);

            return result.Match(Ok, Problem);
        }

        [HttpGet("{id:int}/results")]
        public async Task<IActionResult> GetVotingResults([FromRoute]uint id)
        {
            var result = await getVotingResult.Execute(id);

            return result.Match(Ok, Problem);
        }


        [HttpDelete("{id:int}/candidate")]
        public async Task<IActionResult> DeleteCandidate([FromRoute] uint id, DeleteCandidateRequest request)
        {
            var result = await deleteCandidate.Execute(id, request);

            return result.Match(Ok, Problem);
        }
    }
}
