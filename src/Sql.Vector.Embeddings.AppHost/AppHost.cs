using Aspire.Hosting;
using Aspire.Hosting.DevTunnels;
using CommunityToolkit.Aspire.Hosting.PowerShell;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

/*
 Links for in-progress work
 ==========================
  
 - Function integration - https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-aspire-integration
    - nuget Aspire.Hosting.Azure.Functions

 - Ollama integration - https://aspire.dev/integrations/ai/ollama/
    - nuget CommunityToolkit.Aspire.Hosting.Ollama

*/
//builder.AddProject<Projects.Sql_Vector_Embeddings_BlobUploadConsole>("blob-upload-console");

var aiModelParameter = builder.Configuration[$"Parameters:model"];
var aiEmbeddingModelParameter = builder.Configuration[$"Parameters:embeddingModel"];
var aiEmbeddingDimensionsParameter = builder.Configuration[$"Parameters:embeddingDimensions"];
var useGPUParameter = builder.Configuration[$"Parameters:useGPU"];

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI();

if (bool.TryParse(useGPUParameter, out var useGPU) && useGPU)
{
    ollama.WithGPUSupport();
}

var chatModel = ollama.AddModel("chat", aiModelParameter!);
var embeddingModel = ollama.AddModel("embeddings", aiEmbeddingModelParameter!);

var devTunnel = builder.AddDevTunnel("ollama-api")
   .WithReference(ollama
       //, new DevTunnelPortOptions
       //{
       //    Protocol = "https"
       //}
   )
   //.WithAnonymousAccess()
   .WaitFor(ollama);

var sqlServer = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var sqlDatabase = sqlServer.AddDatabase("database", "Documents");

var ps = builder.AddPowerShell("ps")
    //.WithReference(devTunnel)
    .WithReference(sqlDatabase)
    //.WithReference(devTunnel)
    //.WithReference(devTunnel.GetEndpoint(ollama, "https"), "dev-endpoint");
    //.WithReference(devTunnel.GetEndpoint(ollama, "https"));
    //.WithEnvironment("OLLAMA_API_URL", devTunnel.GetEndpoint("x")!.Url)
    //.WithEnv(devTunnel.GetEndpoint("x"))
    .WaitFor(devTunnel)
    .WaitFor(sqlDatabase);

//var devTunnelConnection = devTunnel.GetEndpoint.GetConnectionInfo();
//ps.WithEnvironment("OLLAMA_API_URL", devTunnelConnection!.Url)
//  .WithEnvironment

///https://github.com/dotnet/docs-aspire/issues/2340
var x = devTunnel.OnResourceEndpointsAllocated(
//builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>(
    (endpoint, @event, cancellationToken) =>
    {
        //Console.WriteLine($"Endpoint allocated: {endpoint.IsAllocated}");
        //Console.WriteLine($"Resolved Url: {endpoint.Url}");

        var endpoint1 = devTunnel.GetEndpoint("http");
        var endpoint1s = devTunnel.GetEndpoint("https");

        return Task.CompletedTask;
    });

var x2 = devTunnel.OnResourceReady(
//builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>(
    (endpoint, @event, cancellationToken) =>
    {
        //Console.WriteLine($"Endpoint allocated: {endpoint.IsAllocated}");
        //Console.WriteLine($"Resolved Url: {endpoint.Url}");

        var tunnelEndpoint = endpoint.GetEndpoint("tunnel");
        var ollamaEndpoint = endpoint.GetEndpoint("tunnel");
        var tunnelEndpoint2 = devTunnel.GetEndpoint("tunnel");
        var tunnelEndpoint3 = ollama.GetEndpoint("tunnel");

        return Task.CompletedTask;
    });

#pragma warning disable CA1031 // Do not catch general exception types
try
{
    var embeddingDiemensions = aiEmbeddingDimensionsParameter is not null && int.TryParse(aiEmbeddingDimensionsParameter, out var dimensions) ? dimensions : 1536;
    var endpoint = devTunnel.GetEndpoint("http");
    var endpoint2 = devTunnel.GetEndpoint("https");
    //var embeddingEndpoint = endpoint?.Url ?? "unknown";
    //var embeddingEndpoint2 = ollama.GetEndpoint("http")?.Url ?? "unknown";
    //var embeddingEndpoint = ollama.GetEndpoint("https")?.Url ?? "unknown";
    //var embeddingEndpoint = devTunnel.GetEndpoint("https")?.Url ?? "unknown";

    var dbInitialisationScript = ps.AddScript("db-ps-script",
        """
        #param($embeddingEndpoint, $dim)
        param($dim)
    
        # Write-Information "Embedding endpoint: $embeddingEndpoint"
        Write-Information "Embedding dimensions: $dim"
        Write-Information "`$database is $database"
    
        # Call SQL script here

        Write-Information "SQL database initialised"
        """)
        .WithArgs(
            //ollama.GetEndpoint("tunnel").Url,
            //GetEmbeddingUrl(),
            embeddingDiemensions)
        .WaitFor(devTunnel)
        .OnBeforeResourceStarted((s, e, c) =>
        {
            Console.WriteLine("Script starting");
            return Task.CompletedTask;
        })
        .OnResourceStopped((s, e, c) =>
        {
            Console.WriteLine("Script stopped");
            return Task.CompletedTask;
        })
        .OnInitializeResource((s, e, c) =>
        {
            Console.WriteLine("Script initializing");
            return Task.CompletedTask;
        })
        .OnResourceReady((s, e, c) =>
        {
            Console.WriteLine("Script ready");
            return Task.CompletedTask;
        });
}
catch (Exception ex)
{
    Console.WriteLine($"Error setting up database initialisation script: {ex}");
}
#pragma warning restore CA1031 // Do not catch general exception types

builder.AddAzureFunctionsProject<Projects.Sql_Vector_Embeddings_Ingestion_Functions>("ingestion-functions")
    .WithReference(sqlDatabase)
    //.WaitFor(sql)
    //.WaitForCompletion(dbInitialisationScript)
    ;

string[] queryPromptArgs = [];
builder.AddProject<Projects.Sql_Vector_Embeddings_QueryConsole>("query-console")
    .WithReference(chatModel)
    .WithReference(embeddingModel)
    .WithReference(sqlDatabase)
    .WithEnvironment("AISettings:embeddingDimensions", aiEmbeddingDimensionsParameter)
    //.WaitFor(chatModel)
    //.WaitFor(embeddingModel)
    //.WaitFor(sql)
    .WithExplicitStart()
    .WithArgs(context =>
    {
        context.Args.Clear();        
        foreach (var arg in queryPromptArgs)
        {
            context.Args.Add(arg);
        }
    })
    .WithCommand("run", "Run query console", async context =>
    {
        //TODO: Try to get an interactive command app
        //      https://rasper87.blog/2025/10/29/building-a-dynamic-command-sample-with-net-aspire/

#pragma warning disable ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();
        var interactionService = context.ServiceProvider.GetRequiredService<IInteractionService>();
        var rns = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();

        var result = await interactionService.PromptInputAsync("Enter the arguments",
            "Enter the arguments for the command to run",
            new()
            {
                InputType = InputType.Text,
                Label = "Arguments",
                Name = "Query arguments",
                Placeholder = "arg1 arg2 arg3",
            });

        if (result.Canceled)
        {
            return CommandResults.Success();
        }

        //queryPromptArgs = result.Data.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        queryPromptArgs = result.Data.Value is not null ? [result.Data.Value] : [];

        if (rns.TryGetCurrentState(context.ResourceName, out var state)
               && state.Snapshot.State?.Text == KnownResourceStates.NotStarted)
        {
            return await commandService.ExecuteCommandAsync(context.ResourceName, "resource-start");
        }

        return await commandService.ExecuteCommandAsync(context.ResourceName, "resource-restart");
#pragma warning restore ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        },
        new()
        {
            IconName = "Play",
            IconVariant = IconVariant.Regular,
            IsHighlighted = true
        });

await builder.Build().RunAsync();

string GetEmbeddingUrl() => 
    ollama.GetEndpoint("tunnel")?.Url ?? "not ready";
