using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sql.Vector.Embeddings.Ingestion.Functions.Models;

namespace Sql.Vector.Embeddings.Ingestion.Functions;

public class IngestFromUriFunction(ILogger<IngestFromUriFunction> logger)
{
    private readonly ILogger<IngestFromUriFunction> _logger = logger;

    private static readonly Action<ILogger, int, Exception?> _logFunctionTriggered =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(0, nameof(IngestFromUriFunction)),
            "IngestFromUriFunction http function triggered with {Count} uris.");

    private static readonly Action<ILogger, Exception?> _logNullUriParameterWarning =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(0, nameof(IngestFromUriFunction)),
            "IngestFromUriFunction called with no URIs.");

    private static readonly Action<ILogger, Uri, Exception?> _logUriProcessStarted =
    LoggerMessage.Define<Uri>(
        LogLevel.Information,
        new EventId(0, nameof(IngestFromUriFunction)),
        "IngestFromUriFunction processing uri: {Uri}.");

    [Function("IngestFromUriFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ingest-uris")]
        HttpRequest _,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        UriListRequest uris)
    {
        if (uris?.Uris is null || uris.Uris.Count == 0)
        {
            _logNullUriParameterWarning(_logger, null);
            return new BadRequestObjectResult("No URIs provided.");
        }

        _logFunctionTriggered(_logger, uris.Uris.Count, null);

        foreach (var uri in uris.Uris)
        {
            _logUriProcessStarted(_logger, uri, null);
        }

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}