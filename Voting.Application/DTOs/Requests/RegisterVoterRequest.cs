﻿namespace Voting.Application.DTOs.Requests
{
    public class RegisterVoterRequest
    {
        public uint SessionId { get; set; }
        public Guid UserId { get; set; }
    }
}
