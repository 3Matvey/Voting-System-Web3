using System.Text.Json.Serialization;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Application.DTOs.Requests
{
    /// <summary>Запрос на создание сессии голосования.</summary>
    public class CreateVotingSessionRequest
    {
        public Guid AdminUserId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegistrationMode Mode { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VerificationLevel RequiredVerificationLevel { get; set; }
    }
}
