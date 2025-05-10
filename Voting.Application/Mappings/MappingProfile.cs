using AutoMapper;
using Voting.Application.DTOs.Responses;
using Voting.Domain.Aggregates;
using Voting.Domain.Entities;

namespace Voting.Application.Mappings
{
    /// <summary>Настраивает маппинг доменных сущностей в DTO.</summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<VotingSessionAggregate, CreateVotingSessionResponse>();

            CreateMap<VotingSessionAggregate, VotingSessionResponse>()
            .ForMember(dest => dest.SessionId,
                       opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Candidates,
                       opt => opt.MapFrom(src => src.Candidates))
            .ForMember(dest => dest.RegisteredVoterIds,
                     opt => opt.MapFrom(src => src.RegisteredUserIds));

            CreateMap<Candidate, CandidateDto>()
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.VoteCount, opt => opt.MapFrom(src => src.VoteCount));

            //    // Маппинг кандидата в DTO результатов кандидата
            //    CreateMap<Candidate, CandidateResultDto>()
            //        .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Id))
            //        .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Name))
            //        .ForMember(dest => dest.VoteCount, opt => opt.MapFrom(src => src.VoteCount));

            //    // Маппинг VotingSession в VotingResultsDto 
            //    CreateMap<VotingSessionAggregate, VotingResultsDto>()
            //        .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
            //        .ForMember(dest => dest.Candidates, opt => opt.MapFrom(src => src.Candidates));

            //    // Маппинг VotingSession в VotingStatusDto с вычислением динамических значений
            //    CreateMap<VotingSession, VotingStatusDto>()
            //        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.VotingActive && DateTime.UtcNow < src.EndTime))
            //        .ForMember(dest => dest.TimeLeft, opt => opt.MapFrom(src => (src.VotingActive && DateTime.UtcNow < src.EndTime)
            //                                            ? (uint)(src.EndTime - DateTime.UtcNow).TotalSeconds
            //                                            : 0))
            //        .ForMember(dest => dest.TotalVotesCount, opt => opt.MapFrom(src => (uint)src.Candidates.Sum(c => c.VoteCount)));
            //}
        }
    }
}


