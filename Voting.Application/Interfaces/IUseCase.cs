namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Общий интерфейс для всех Use Case’ов.
    /// </summary>
    public interface IUseCase<TRequest, TResult>
    {
        Task<Result<TResult>> Execute(TRequest request);
    }
}
