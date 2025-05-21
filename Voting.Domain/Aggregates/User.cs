using System.ComponentModel.DataAnnotations;
using Voting.Domain.Common;
using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Events;

namespace Voting.Domain.Aggregates
{
    public sealed class User(Guid id, string email, string blockchainAddress, Role role) 
        : AggregateRoot
    {
        public Guid Id { get; private set; } = id;
        [EmailAddress]
        public string Email { get; private set; } = email ?? throw new ArgumentNullException(nameof(email));
        public string BlockchainAddress { get; private set; } = blockchainAddress ?? throw new ArgumentNullException(nameof(blockchainAddress));
        public Role Role { get; private set; } = role;
        public VerificationLevel VerificationLevel { get; private set; } = VerificationLevel.None;

        public string PasswordHash { get; private set; } = string.Empty;
        public string PasswordSalt { get; private set; } = string.Empty;

        public void SetPassword(string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Password hash must not be empty.", nameof(hash));
            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Password salt must not be empty.", nameof(salt));

            PasswordHash = hash;
            PasswordSalt = salt;

            // Опционально: доменное событие смены/установки пароля
            AddDomainEvent(new UserPasswordSetDomainEvent(Id));
        }

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
