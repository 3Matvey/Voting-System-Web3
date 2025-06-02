// Voting.Application.UseCases.Commands.User/VerifyPassportUseCase.cs
using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.User
{
    /// <summary>
    /// Use Case для верификации паспорта.
    /// </summary>
    public class VerifyPassportUseCase(IUnitOfWork uow)
    {
        public async Task<Result> Execute(Guid userId, PassportIdentifier passport)
        {
            if (userId == Guid.Empty)
                return Error.Validation("InvalidRequest", "userId обязателен.");


            var user = await uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"Пользователь {userId} не найден.");

            // Применяем доменную логику
            user.VerifyPassport(passport);

            await uow.CommitAsync();
            return Result.Success();
        }
    }
}
