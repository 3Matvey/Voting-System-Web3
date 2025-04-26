namespace Voting.Domain.Entities.ValueObjects
{
    /// <summary>
    /// Режим регистрации участников голосования:
    /// – Invitation – только по приглашению (админ выдаёт invite-link);
    /// – SelfCustody – любая подпись через Web3-кошелёк.
    /// </summary>
    public enum RegistrationMode
    {
        Invitation,
        SelfCustody
    }
}
