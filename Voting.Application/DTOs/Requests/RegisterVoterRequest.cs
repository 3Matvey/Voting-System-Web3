using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voting.Application.DTOs.Requests
{
    public class AddCandidateRequest
    {
        public uint SessionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
