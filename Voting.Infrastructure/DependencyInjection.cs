using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Voting.Application.Interfaces;

namespace Voting.Infrastructure.Blockchain
{
    public class BlockchainOptions
    {
        public string RpcUrl { get; set; } = string.Empty;
        public string WsUrl { get; set; } = string.Empty;
        public string ContractAddress { get; set; } = string.Empty;
    }

    public static class DependencyInjection
    {
        /// <summary>
        /// Регистрирует все «infrastructure»‑сервисы для работы с блокчейном.
        /// Ожидается секция "Blockchain" в конфиге.
        /// </summary>
        public static IServiceCollection AddBlockchainServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1) Привязываем секцию "Blockchain" к нашим опциям
            services.Configure<BlockchainOptions>(
                configuration.GetSection("Blockchain"));

            // 2) Адаптер работы с контрактом (RPC)
            services.AddSingleton<ISmartContractAdapter>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<BlockchainOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<SmartContractAdapter>>();
                return new SmartContractAdapter(
                    rpcUrl: opts.RpcUrl,
                    contractAddress: opts.ContractAddress,
                    logger: logger);
            });

            // 3) Слушатель событий (WebSocket)
            services.AddSingleton<IContractEventListener>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<BlockchainOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<ContractEventListener>>();
                return new ContractEventListener(
                    wsUrl: opts.WsUrl,
                    contractAddress: opts.ContractAddress,
                    logger: logger);
            });

            return services;
        }
    }
}
