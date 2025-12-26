using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sql.Vector.Embeddings.Ingestion.Functions.Models;

namespace Sql.Vector.Embeddings.Ingestion.Functions;

public class IngestFromUriFunction(ILogger<IngestFromUriFunction> logger)
{
    ILogger<IngestFromUriFunction> _logger = logger;

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

    [Function("IngestFromUriFunction")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ingest-uris")] 
        HttpRequest request,
        UriListRequest uriList)
    {
        if(uriList?.Items == null || uriList.Items.Count == 0)
        {
            _logNullriParameterWarning(_logger, null);
            return new BadRequestObjectResult("No URIs provided.");
        }

        _logFunctionTriggered(_logger, uriList.Items.Count, null);
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}