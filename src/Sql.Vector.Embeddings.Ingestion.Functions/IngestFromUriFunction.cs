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

    private static readonly Action<ILogger, Exception?> _logNullriParameterWarning =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(0, nameof(IngestFromUriFunction)),
            "IngestFromUriFunction http function called with no URIs.");

    private static readonly Action<ILogger, Uri, Exception?> _logUriProcessStarted =
    LoggerMessage.Define<Uri>(
        LogLevel.Information,
        new EventId(0, nameof(IngestFromUriFunction)),
        "IngestFromUriFunction processing uri: {Uri}.");

    [Function("IngestFromUriFunction")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ingest-uris")]
        HttpRequest request,
        UriListRequest uris)
    {
        //https://stackoverflow.com/questions/76013830/net-azure-functions-model-binding

        if (uris?.Uris == null || uris.Uris.Count == 0)
        {
            _logNullriParameterWarning(_logger, null);
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