namespace Voting.Shared.ResultPattern;

/// <summary>
/// Represents the types of errors that can occur in the application.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    NotFound = 1,
    Validation = 2,
    Conflict = 3,
    AccessUnauthorized = 4,
    AccessForbidden = 5,
}
