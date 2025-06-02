using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    /// <summary>
    /// Админ приглашает конкретного пользователя (в режиме Invitation).
    /// </summary>
    public class InviteUserUseCase(IUnitOfWork uow)
    {
        public async Task<Result> Execute(uint sessionId, Guid adminUserId, Guid targetUserId)
        {
            if (sessionId == 0 || adminUserId == Guid.Empty || targetUserId == Guid.Empty)
                return Error.Validation("InvalidRequest", "sessionId, adminUserId и targetUserId обязательны.");

            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session == null)
                return Error.NotFound("VotingSession.NotFound", $"Сессия {sessionId} не найдена.");

            if (session.AdminUserId != adminUserId)
                return Error.AccessForbidden("VotingSession.Forbidden", "Только админ может приглашать.");

            if (session.Mode != RegistrationMode.Invitation)
                return Error.Validation("VotingSession.InvalidMode", "Приглашать можно только в режиме Invitation.");

            var user = await uow.Users.GetByIdAsync(targetUserId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"Пользователь {targetUserId} не найден.");

            if (user.VerificationLevel < session.RequiredVerificationLevel)
                return Error.Validation(
                    "User.InsufficientVerification",
                    $"Уровень верификации пользователя ({user.VerificationLevel}) ниже требуемого ({session.RequiredVerificationLevel})."
                );

            if (session.RegisteredUserIds.Contains(targetUserId))
                return Error.Validation("User.AlreadyRegistered", "Пользователь уже зарегистрирован или голосовал.");

            try
            {
                session.RegisterVoter(user);
            }
            catch (DomainException ex)
            {
                return Error.Validation("RegistrationError", ex.Message);
            }

            return Result.Success();
        }
    }
}
