namespace Voting.Domain.Entities
{
    public record User(
        Guid Id, 
        string Username,
        string BlockchainAddress,
        Role Role
    );
}
