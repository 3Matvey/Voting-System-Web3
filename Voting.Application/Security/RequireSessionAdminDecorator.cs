using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Voting.Application.Interfaces;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces;
using Voting.Shared.ResultPattern;

namespace Voting.Application.Security
{
    /// <summary>
    /// Декоратор, который передавая управление внутрь UseCase,
    /// вначале проверяет RequireSessionAdminAttribute (если он есть).
    /// </summary>
    public class RequireSessionAdminDecorator<TRequest, TResult>(
        IUseCase<TRequest, TResult> inner,
        IUnitOfWork uow)
        : IUseCase<TRequest, TResult>
    {
        private readonly IUseCase<TRequest, TResult> _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));

        public async Task<Result<TResult>> Execute(TRequest request)
        {
            var type = _inner.GetType();
            var attr = type.GetCustomAttribute<RequireSessionAdminAttribute>();
            if (attr != null)
            {
                // читаем из request sessionId и adminUserId
                var reqType = typeof(TRequest);

                var sidProp = reqType.GetProperty(attr.SessionIdProperty,
                    BindingFlags.Public | BindingFlags.Instance);
                var admProp = reqType.GetProperty(attr.AdminUserIdProperty,
                    BindingFlags.Public | BindingFlags.Instance);

                if (sidProp == null || admProp == null)
                {
                    return Error.Validation(
                        "AuthConfigError",
                        $"RequireSessionAdmin: cannot find properties " +
                        $"{attr.SessionIdProperty} or {attr.AdminUserIdProperty} on {reqType.Name}");
                }

                var sessionId = (uint)(sidProp.GetValue(request) ?? 0u);
                var adminUserId = (Guid)(admProp.GetValue(request) ?? Guid.Empty);

                // off-chain проверка
                var session = await _uow.VotingSessions.GetByIdAsync(sessionId);
                if (session == null)
                    return Error.NotFound(
                        "SessionNotFound",
                        $"Voting session {sessionId} not found.");

                if (session.AdminUserId != adminUserId)
                    return Error.AccessForbidden(
                        "Forbidden",
                        "Only session admin can perform this operation.");
            }

            // без атрибута или после успешной проверки — выполняем сам UseCase
            return await _inner.Execute(request);
        }
    }
}
