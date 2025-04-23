using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Voting.Application.Interfaces;

namespace Voting.Infrastructure.Blockchain
{
    file class BlockchainOptions
    {
        public string RpcUrl { get; set; } = string.Empty;
        public string WsUrl { get; set; } = string.Empty;
        public string ContractAddress { get; set; } = string.Empty;
        public string DefaultSenderAddress { get; set; } = string.Empty;
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

            // 2) Слушатель событий (WebSocket) — он нужен адаптеру
            services.AddSingleton<IContractEventListener>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<BlockchainOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<ContractEventListener>>();
                return new ContractEventListener(
                    wsUrl: opts.WsUrl,
                    contractAddress: opts.ContractAddress,
                    logger: logger
                );
            });

            // 3) Адаптер работы с контрактом (RPC) — теперь зависит от IContractEventListener
            services.AddSingleton<ISmartContractAdapter>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<BlockchainOptions>>().Value;
                var listener = sp.GetRequiredService<IContractEventListener>();
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<SmartContractAdapter>>();
                return new SmartContractAdapter(
                    rpcUrl: opts.RpcUrl,
                    defaultSenderAddress: opts.DefaultSenderAddress,
                    contractAddress: opts.ContractAddress,
                    listener: listener,
                    logger: logger
                );
            });

            return services;
        }
    }
}
