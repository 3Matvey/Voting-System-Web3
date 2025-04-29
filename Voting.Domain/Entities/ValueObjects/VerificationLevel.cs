namespace Voting.Domain.Entities.ValueObjects
{
    [Flags]
    public enum VerificationLevel : byte
    {
        None = 0b0000_0000,
        Email = 0b0000_0001,
        PhoneNumber = Email << 1,
        Social = Email << 2,
        Passport = Email << 3,   
    }
}