namespace Voting.Infrastructure.Blockchain.ContractFunctions
{
    [Function("createSession", "uint256")]
    internal class CreateSessionFunction : FunctionMessage
    {
        [Parameter("address", "sessionAdmin", 1)]
        public string SessionAdmin { get; set; } = string.Empty;
    }
}
