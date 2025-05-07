namespace Voting.Application.Security
{
    /// Помечает UseCase, что перед выполнением требуется
    /// проверить, что request.AdminUserId является админом request.SessionId.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RequireSessionAdminAttribute(
        string sessionIdProperty = "SessionId",
        string adminUserIdProperty = "AdminUserId") : Attribute
    {
        /// <summary>Имя свойства в TRequest с sessionId.</summary>
        public string SessionIdProperty { get; } = sessionIdProperty;

        /// <summary>Имя свойства в TRequest с AdminUserId.</summary>
        public string AdminUserIdProperty { get; } = adminUserIdProperty;
    }
}

