// Voting.Application/UseCases/Commands/Auth/RegisterUseCase.cs
using System.ComponentModel.DataAnnotations;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Entities.ValueObjects;
using Voting.Domain.Events;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.Auth
{
    public class RegisterUseCase(
        IUnitOfWork uow,
        IJwtTokenService jwt,
        IPasswordHasher hasher,
        IDomainEventPublisher publisher)
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly IJwtTokenService _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        private readonly IPasswordHasher _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        private readonly IDomainEventPublisher _publisher
                                          = publisher ?? throw new ArgumentNullException(nameof(publisher));

        public async Task<Result<AuthResponse>> Execute(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request must not be null.");

            if (!new EmailAddressAttribute().IsValid(request.Email))
                return Error.Validation("InvalidEmail", "Email is not a valid address.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return Error.Validation("InvalidPassword", "Password must be at least 6 characters.");

            // Проверяем, что такого email ещё нет
            var existing = await _uow
                .Users
                .GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null)
                return Error.Validation("EmailExists", "This email is already registered.");

            // Хешируем пароль
            var (hash, salt) = _hasher.Hash(request.Password);

            // Создаём аггрегат
            var user = new Domain.Aggregates.User(
                Guid.NewGuid(),
                request.Email,
                request.BlockchainAddress,
                Role.Voter
            );

            // Устанавливаем хеш и соль
            user.SetPassword(hash, salt);

            // Публикуем доменное событие (опционально)
            _publisher.Publish(new UserRegisteredDomainEvent(user.Id, user.Email));

            // Сохраняем и коммитим
            await _uow.Users.AddAsync(user, cancellationToken);
            await _uow.CommitAsync();

            // Генерируем JWT + refresh
            var tokens = _jwt.CreateTokens(user.Id, user.Role.ToString());

            // Формируем ответ
            var response = new AuthResponse
            {
                UserId = user.Id,
                Token = tokens.Token,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt
            };

            return Result<AuthResponse>.Success(response);
        }
    }
}
