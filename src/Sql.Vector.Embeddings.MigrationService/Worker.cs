using System.Collections;
using System.Diagnostics;

namespace Sql.Vector.Embeddings.MigrationService;

public class Worker(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    private static readonly Action<ILogger, DateTimeOffset, Exception?> _logWorkerStarted =
        LoggerMessage.Define<DateTimeOffset>(
            LogLevel.Information,
            new EventId(0, nameof(ExecuteAsync)),
            "Worker running at: {Time}");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logWorkerStarted(logger, DateTimeOffset.Now, null);

        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();

            Console.WriteLine("\n======================");
            Console.WriteLine($"Configuration:");
            var embeddingDimensions = int.TryParse(configuration["AISettings:embeddingDimensions"], out var dimensions) && dimensions > 0 ? dimensions : 1536;
            var embeddingModel = configuration["AISettings:embeddingModel"];
            var ollamaTunnel = configuration["services:ollama:http:0"];
            var ollamaEndpoint = configuration["OLLAMA_HTTP"];

            Console.WriteLine($"  Embedding model {embeddingModel}");
            Console.WriteLine($"  Embedding dimensions: {embeddingDimensions}");
            Console.WriteLine($"  Ollama tunnel: {ollamaTunnel}");
            Console.WriteLine($"  Ollama endpoint: {ollamaEndpoint}");

            Console.WriteLine($"All configuration:");
            foreach (var config in configuration.AsEnumerable())
            {
                Console.WriteLine($"  {config.Key}:{config.Value}");
            }

            Console.WriteLine($"Connection strings:");
            var connections = configuration.GetSection("ConnectionStrings");
            if (connections is not null)
            {
                foreach (var conn in connections.AsEnumerable())
                {
                    Console.WriteLine($"  {conn.Key}:{conn.Value}");
                }
            }
            Console.WriteLine("======================\n");

            Console.WriteLine("\n======================");
            Console.WriteLine($"Environment variables:");
            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"  {e.Key}:{e.Value}");
            }
            Console.WriteLine("======================\n");

            //var dbContext = scope.ServiceProvider.GetRequiredService<TicketContext>();

            //await RunMigrationAsync(dbContext, cancellationToken);
            //await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
}
