using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

builder.AddSqlServerClient("sql");

//See https://github.com/yorek/azure-sql-db-ai-samples-search/blob/main/db-scripts/Program.cs
//    https://devblogs.microsoft.com/azure-sql/efficiently-and-elegantly-modeling-embeddings-in-azure-sql-and-sql-server/

Console.WriteLine($"Configuration:");
var embeddingDimensions = int.TryParse(builder.Configuration["AISettings:embeddingDimensions"], out var dimensions) && dimensions > 0 ? dimensions : 1536;
var embeddingModel = builder.Configuration["AISettings:embeddingModel"];
var ollamaTunnel = builder.Configuration["services:ollama:http:0"];
var ollamaEndpoint = builder.Configuration["OLLAMA_HTTP"];

Console.WriteLine($"  Embedding model {embeddingModel}");
Console.WriteLine($"  Embedding dimensions: {embeddingDimensions}");
Console.WriteLine($"  Ollama tunnel: {ollamaTunnel}");
Console.WriteLine($"  Ollama endpoint: {ollamaEndpoint}");

var connections = builder.Configuration.GetSection("ConnectionStrings");
if (connections is not null)
{
    foreach (var conn in connections.AsEnumerable())
    {
        Console.WriteLine($"  {conn.Key}:{conn.Value}");
    }
}
