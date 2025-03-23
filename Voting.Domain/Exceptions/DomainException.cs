namespace Voting.Domain.Exceptions
{
    public class DomainException(string message) 
        : Exception(message) { }
}
