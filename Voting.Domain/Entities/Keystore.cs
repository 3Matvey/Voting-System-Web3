namespace Voting.Domain.Entities
{
    public class Keystore(Guid userId, uint sessionId, string jsonVault, string address)
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid UserId { get; } = userId;
        public uint SessionId { get; } = sessionId;
        public string JsonVault { get; } = jsonVault ?? throw new ArgumentNullException(nameof(jsonVault));
        public string Address { get; } = address ?? throw new ArgumentNullException(nameof(address));
    }
}