using Aspire.Hosting;

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

var ollama = builder.AddOllama("ollama");
if (bool.TryParse(useGPUParameter, out var useGPU) && useGPU)
{
    ollama.WithGPUSupport();
}

var embeddingDiemensions = aiEmbeddingDimensionsParameter is not null && int.TryParse(aiEmbeddingDimensionsParameter, out var dimensions) ? dimensions : 1536;

var chatModel = ollama.AddModel("chat", aiModelParameter!);
var embeddingModel = ollama.AddModel("embeddings", aiEmbeddingModelParameter!);

builder.AddAzureFunctionsProject<Projects.Sql_Vector_Embeddings_Ingestion_Functions>("ingestion-functions");

builder.AddProject<Projects.Sql_Vector_Embeddings_QueryConsole>("query-console")
    .WithReference(chatModel)
    .WithReference(embeddingModel)
    .WithEnvironment("AISettings:embeddingDimensions", aiEmbeddingDimensionsParameter)
    .WaitFor(chatModel)
    .WaitFor(embeddingModel)
    ;

await builder.Build().RunAsync();
