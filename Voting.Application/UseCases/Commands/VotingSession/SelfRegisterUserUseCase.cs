using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.VotingSession
{
    /// <summary>
    /// Пользователь сам регистрируется в сессию (режим SelfCustody).
    /// </summary>
    public class SelfRegisterUserUseCase(IUnitOfWork uow)
    {
        public async Task<Result> Execute(uint sessionId, Guid userId)
        {
            if (sessionId == 0 || userId == Guid.Empty)
                return Error.Validation("InvalidRequest", "sessionId и userId обязательны.");

            // 1) Сессия
            var session = await uow.VotingSessions.GetByIdAsync(sessionId);
            if (session == null)
                return Error.NotFound("VotingSession.NotFound", $"Сессия {sessionId} не найдена.");

            // 2) Режим должен быть SelfCustody
            if (session.Mode != RegistrationMode.SelfCustody)
                return Error.Validation("VotingSession.InvalidMode", "Саморегистрация возможна только в режиме SelfCustody.");

            // 3) Проверяем, что пользователь есть
            var user = await uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"Пользователь {userId} не найден.");

            // 4) Проверяем, что уровень верификации достаточен
            if (user.VerificationLevel < session.RequiredVerificationLevel)
                return Error.Validation(
                    "User.InsufficientVerification",
                    $"Уровень верификации ({user.VerificationLevel}) ниже требуемого ({session.RequiredVerificationLevel})."
                );

            // 5) Проверяем, что пользователь еще не зарегистрирован
            if (session.RegisteredUserIds.Contains(userId))
                return Error.Validation("User.AlreadyRegistered", "Пользователь уже зарегистрирован или голосовал.");

            // 6) Регистрируем
            try
            {
                session.RegisterVoter(user);
            }
            catch (DomainException ex)
            {
                return Error.Validation("RegistrationError", ex.Message);
            }

            await uow.CommitAsync();
            return Result.Success();
        }
    }
}
