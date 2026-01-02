using CommunityToolkit.Aspire.Hosting.PowerShell;
using Microsoft.Extensions.DependencyInjection;
using Sql.Vector.Embeddings.AppHost.Extensions;

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
var gpuVendorParameter = builder.Configuration[$"Parameters:OllamaGpuVendor"];

var ollama = builder.AddOllama("ollama")
    .WithGPUSupportIfVendorParameterProvided(gpuVendorParameter)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithOpenWebUI();

var chatModel = ollama.AddModel("chat", aiModelParameter!);
var embeddingModel = ollama.AddModel("embeddings", aiEmbeddingModelParameter!);

var devTunnel = builder.AddDevTunnel("ollama-api")
   .WithReference(ollama
       //, new DevTunnelPortOptions
       //{
       //    Protocol = "https"
       //}
   )
   .WithAnonymousAccess()
   .WaitFor(ollama)
   //https://github.com/dotnet/docs-aspire/issues/2340
    .OnResourceEndpointsAllocated((endpoint, @event, cancellationToken) =>
    {
        //Console.WriteLine($"Endpoint allocated: {endpoint.IsAllocated}");
        //Console.WriteLine($"Resolved Url: {endpoint.Url}");
        return Task.CompletedTask;
    })
    .OnResourceReady((endpoint, @event, cancellationToken) =>
    {
        return Task.CompletedTask;
    });

var sql = builder.AddSqlServer("sql")
    .WithImage("mssql/server", "2025-latest")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("database", "Documents");

var ps = builder.AddPowerShell("ps")
    //.WithReference(ollama, devTunnel)
    //.WithReference(devTunnel.GetEndpoint(ollama, "https"));
    .WaitFor(devTunnel)
    .WaitFor(sql);

#pragma warning disable CA1031 // Do not catch general exception types
try
{
    var embeddingDiemensions = aiEmbeddingDimensionsParameter is not null && int.TryParse(aiEmbeddingDimensionsParameter, out var dimensions) ? dimensions : 1536;
    ps.AddScript("db-ps-script",
        """
        #param($embeddingEndpoint, $dim)
        param($embeddingModel, $dim)
    
        # Write-Information "Embedding endpoint: $embeddingEndpoint"
        Write-Information "Embedding model: $embeddingModel"
        Write-Information "Embedding dimensions: $dim"
        Write-Information ('$dim is ' + $dim)
        Write-Information "database is $database"
        #Write-Information "ep is $ep"
        #Write-Information "eps is $eps"
    
        # List environment variables
        Write-Information "Environment variables:"
        Get-ChildItem Env:

        $testing = $env:TESTENVVAR
        Write-Information "Test env var: $testing"
        
        $embeddingEndpoint = $env:services__ollama__https__0
        Write-Information "Ollama Endpoint: $embeddingEndpoint"

        # Call SQL script here

        Write-Information "SQL database initialised"

        """)
        .WithEnvironment("TESTENVVAR", "Hello world")
        .WithArgs(
            //GetEmbeddingUrl(),
            aiEmbeddingModelParameter ?? "",
            embeddingDiemensions
            //ollama.GetEndpoint("http")?.Url ?? "unknown",
            //devTunnel.GetEndpoint("https")?.Url ?? "unknown"
            )
        //.WaitFor(devTunnel)
        //.WaitFor(devTunnel)
        .WithReference(ollama, devTunnel)
        .WithReference(embeddingModel)
        .WithReference(sql)
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

var migrations = builder.AddProject<Projects.Sql_Vector_Embeddings_MigrationService>("migrations")
    .WithReference(sql)
    .WithReference(ollama, devTunnel)
    .WithEnvironment("AISettings:embeddingDimensions", aiEmbeddingDimensionsParameter)
    .WithEnvironment("AISettings:embeddingModel", aiEmbeddingModelParameter)
    .WaitFor(sql);

var databaseDeployment = builder.AddProject<Projects.Sql_Vector_Embeddings_DatabaseDeploymentService>("deploy-db")
    .WithReference(sql)
    .WithReference(ollama, devTunnel)
    .WithEnvironment("AISettings:embeddingDimensions", aiEmbeddingDimensionsParameter)
    .WithEnvironment("AISettings:embeddingModel", aiEmbeddingModelParameter)
    .WaitFor(sql);

builder.AddAzureFunctionsProject<Projects.Sql_Vector_Embeddings_Ingestion_Functions>("ingestion-functions")
    .WithReference(sql)
    .WaitForCompletion(databaseDeployment)
    //.WaitForCompletion(migrations)
    ;

string[] queryPromptArgs = [];
builder.AddProject<Projects.Sql_Vector_Embeddings_QueryConsole>("query-console")
    .WithReference(chatModel)
    .WithReference(embeddingModel)
    .WithReference(sql)
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
