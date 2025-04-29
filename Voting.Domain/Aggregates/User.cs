using System.ComponentModel.DataAnnotations;
using Voting.Domain.Common;
using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Events;

namespace Voting.Domain.Aggregates
{
    public sealed class User(Guid id, string email, string blockchainAddress, Role role) 
        : AggregateRoot<Guid>
    {
        public override Guid Id { get; private protected set; } = id;
        [EmailAddress]
        public string Email { get; private set; } = email ?? throw new ArgumentNullException(nameof(email));
        public string BlockchainAddress { get; private set; } = blockchainAddress ?? throw new ArgumentNullException(nameof(blockchainAddress));
        public Role Role { get; private set; } = role;
        public VerificationLevel VerificationLevel { get; private set; } = VerificationLevel.None;

        public void VerifyEmail()
        {
            if (!VerificationLevel.HasFlag(VerificationLevel.Email))
            {
                VerificationLevel |= VerificationLevel.Email;
                AddDomainEvent(new EmailVerifiedDomainEvent(Id));
            }
        }

        public void VerifyPassport(PassportIdentifier passport)
        {
            // валидируется в другом месте
            VerificationLevel |= VerificationLevel.Passport;
            AddDomainEvent(new PassportVerifiedDomainEvent(Id, passport));
        }
    }
}
