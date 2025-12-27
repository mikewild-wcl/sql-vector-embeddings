using Aspire.Hosting;
using k8s.KubeConfigModels;
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

var embeddingDiemensions = aiEmbeddingDimensionsParameter is not null && int.TryParse(aiEmbeddingDimensionsParameter, out var dimensions) ? dimensions : 1536;

var chatModel = ollama.AddModel("chat", aiModelParameter!);
var embeddingModel = ollama.AddModel("embeddings", aiEmbeddingModelParameter!);

builder.AddDevTunnel("ollama-api")
   .WithReference(ollama)   
   .WithAnonymousAccess()
   .WaitFor(ollama);

builder.AddAzureFunctionsProject<Projects.Sql_Vector_Embeddings_Ingestion_Functions>("ingestion-functions");

string[] projectArgs = [];
builder.AddProject<Projects.Sql_Vector_Embeddings_QueryConsole>("query-console")
    .WithReference(chatModel)
    .WithReference(embeddingModel)
    .WithEnvironment("AISettings:embeddingDimensions", aiEmbeddingDimensionsParameter)
    .WaitFor(chatModel)
    .WaitFor(embeddingModel)
    .WithExplicitStart()
           .WithArgs(context =>
           {
               context.Args.Clear();
               foreach (var arg in projectArgs)
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

        projectArgs = result.Data.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

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
