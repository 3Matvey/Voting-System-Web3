using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.User
{
    /// <summary>
    /// Use Case для верификации email пользователя.
    /// </summary>
    public class VerifyEmailUseCase(IUnitOfWork uow)
    {
        public async Task<Result> Execute(Guid userId)
        {
            if (userId == Guid.Empty)
                return Error.Validation("InvalidRequest", "userId обязателен.");

            var user = await uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"Пользователь {userId} не найден.");

            // Если уже есть флаг Email – ничего не делаем
            if (user.VerificationLevel.HasFlag(VerificationLevel.Email))
                return Result.Success();

            // Вызываем доменную логику
            user.VerifyEmail();

            // Сохраняем
            await uow.CommitAsync();
            return Result.Success();
        }
    }
}
