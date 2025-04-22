using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voting.Application;
using Voting.Application.Interfaces;
using Voting.Infrastructure.Blockchain;

// 1) Настраиваем Host
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        // Логирование в консоль
        services.AddLogging(lb => lb.AddSimpleConsole(o =>
        {
            o.TimestampFormat = "[HH:mm:ss] ";
        }));

        // Регистрируем Application‑ и Infrastructure‑слои
        services
            .AddVotingApplication()
            .AddBlockchainServices(ctx.Configuration);
    })
    .Build();

// 2) Получаем IConfiguration, логгер и сервисы из контейнера
var configuration = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var adapter = host.Services.GetRequiredService<ISmartContractAdapter>();
var listener = host.Services.GetRequiredService<IContractEventListener>();

// 3) Подписываемся на события контракта
listener.SessionCreated += (s, e) => logger.LogInformation(
    "Event: SessionCreated    id={SessionId}, admin={SessionAdmin}",
    e.SessionId, e.SessionAdmin);
listener.CandidateAdded += (s, e) => logger.LogInformation(
    "Event: CandidateAdded    session={SessionId}, candidate={CandidateId}, name={Name}",
    e.SessionId, e.CandidateId, e.Name);
listener.CandidateRemoved += (s, e) => logger.LogInformation(
    "Event: CandidateRemoved  session={SessionId}, candidate={CandidateId}",
    e.SessionId, e.CandidateId);
listener.VotingStarted += (s, e) => logger.LogInformation(
    "Event: VotingStarted     session={SessionId}, start={StartTimeUtc}, end={EndTimeUtc}",
    e.SessionId, e.StartTimeUtc, e.EndTimeUtc);
listener.VotingEnded += (s, e) => logger.LogInformation(
    "Event: VotingEnded       session={SessionId}, end={EndTimeUtc}",
    e.SessionId, e.EndTimeUtc);
listener.VoteCast += (s, e) => logger.LogInformation(
    "Event: VoteCast          session={SessionId}, voter={Voter}, candidate={CandidateId}",
    e.SessionId, e.Voter, e.CandidateId);

// 4) Стартуем слушатель событий
logger.LogInformation("Starting ContractEventListener…");
await listener.StartAsync();

// 5) Тестируем адаптер
// 5.1. Создаём новую сессию
var adminAddress = configuration["Blockchain:DefaultSenderAddress"];
if (string.IsNullOrWhiteSpace(adminAddress))
{
    logger.LogError("Не задан DefaultSenderAddress в конфиге.");
    return;
}

logger.LogInformation("Creating new voting session…");
var sessionId = await adapter.CreateSessionAsync(adminAddress);
logger.LogInformation("→ Created session {SessionId}", sessionId);

// 5.2. Добавляем кандидатов
logger.LogInformation("Adding two candidates…");
var tx1 = await adapter.AddCandidateAsync(sessionId, "Alice");
logger.LogInformation("→ Added Alice, tx {Tx}", tx1);
var tx2 = await adapter.AddCandidateAsync(sessionId, "Bob");
logger.LogInformation("→ Added Bob,   tx {Tx}", tx2);

// 5.3. Запускаем голосование на 2 минуты
logger.LogInformation("Starting voting for 2 minutes…");
var tx3 = await adapter.StartVotingAsync(sessionId, 2);
logger.LogInformation("→ Voting started, tx {Tx}", tx3);

// 5.4. Ждём и голосуем
logger.LogInformation("Waiting 10 seconds, then casting a vote…");
await Task.Delay(TimeSpan.FromSeconds(10));
var voterAddress = configuration["Blockchain:VoterAddress"];
if (string.IsNullOrWhiteSpace(voterAddress))
{
    logger.LogError("Не задан VoterAddress в конфиге.");
}
else
{
    var tx4 = await adapter.VoteAsync(sessionId, 1, voterAddress);
    logger.LogInformation("→ Vote cast, tx {Tx}", tx4);
}

// 5.5. Читаем текущее состояние кандидатов
logger.LogInformation("Fetching current candidates…");
var candidates = await adapter.GetCandidatesAsync(sessionId);
foreach (var c in candidates)
{
    logger.LogInformation(
        "Candidate {Id}: {Name}, votes={VoteCount}",
        c.Id, c.Name, c.VoteCount);
}

Console.WriteLine("Press ENTER to end voting…");
Console.ReadLine();

// 5.6. Завершаем голосование
logger.LogInformation("Ending voting session…");
var tx5 = await adapter.EndVotingAsync(sessionId);
logger.LogInformation("→ Voting ended, tx {Tx}", tx5);

Console.WriteLine("Press ENTER to exit program.");
Console.ReadLine();

// 6) Корректно останавливаем слушатель и хост
await listener.StopAsync();
await host.StopAsync();
