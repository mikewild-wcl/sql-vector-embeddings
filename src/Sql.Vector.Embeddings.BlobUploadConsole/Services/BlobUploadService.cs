using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sql.Vector.Embeddings.BlobUploadConsole.Services;

internal partial class BlobUploadService(
    IConfiguration configuration,
    BlobServiceClient blobServiceClient,
    ILogger<BlobUploadService> logger) : IHostedService
{
    private static readonly Action<ILogger, Exception?> _startingLog =
        LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(BlobUploadService)), "BlobUploadService is starting");
    private static readonly Action<ILogger, Exception?> _stoppingLog =
        LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(BlobUploadService)), "BlobUploadService is stopping");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _startingLog(logger, null);
        Console.WriteLine("Starting...");

        await UploadBlobs();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingLog(logger, null);
        Console.WriteLine("Stopping...");
    }

    private async Task UploadBlobs()
    {
        try
        {
            //TODO: Refactor to a cli app
            var path = configuration["fileDirectory"];
            var blobContainer = configuration["blobStorage:companiesHouseContainerName"] ?? throw new InvalidOperationException("Configuration for blobStorage:companiesHouseContainerName not found");
            var userId = configuration["userId"] ?? "unknown";
            var delay = int.TryParse(configuration["delay"], out var delayValue) ? delayValue : 0;

            if(string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Configuration for fileDirectory not found");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Configuration for fileDirectory not found");
            }

            var directory = new DirectoryInfo(path);
            var files = directory.GetFiles("*.zip");

            if (files is { Length: 0 })
            {
                Console.WriteLine("No files found");
                return;
            }

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainer);

            foreach (var file in files)
            {
                if (delay > 0) await Task.Delay(delay);

                var metadata = new Dictionary<string, string>
                {
                    { "fileTypeEPR", "CompaniesHouse" },
                    { "userId", userId },
                    { "fileType", "text/csv" },
                    { "fileName", file.Name }
                };

                using var stream = file.OpenRead();
                var blob = blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());
                await blob.UploadAsync(stream);
                await blob.SetMetadataAsync(metadata);

                Console.WriteLine($"Saved blob {blob.Name} for file {file.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed with exception: {ex.Message}");
            throw;
        }
    }
}
