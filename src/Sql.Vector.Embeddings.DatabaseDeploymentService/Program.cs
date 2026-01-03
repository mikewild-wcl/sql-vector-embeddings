using DbUp;
using DbUp.Extensions.Logging;
using DbUp.ScriptProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

#pragma warning disable CA1848 // Use the LoggerMessage delegates

const string sqlFolder = "./sql";

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
var logger = loggerFactory.CreateLogger<Program>();

//See https://github.com/yorek/azure-sql-db-ai-samples-search/blob/main/db-scripts/Program.cs
//    https://devblogs.microsoft.com/azure-sql/efficiently-and-elegantly-modeling-embeddings-in-azure-sql-and-sql-server/

var embeddingDimensions = int.TryParse(builder.Configuration["AISettings:embeddingDimensions"], out var dimensions) && dimensions > 0 ? dimensions : 1536;
var embeddingModel = builder.Configuration["AISettings:embeddingModel"];
var ollamaTunnel = builder.Configuration["services:ollama:http:0"];
var ollamaEndpoint = builder.Configuration["OLLAMA_HTTP"];

var connectionString = builder.Configuration.GetConnectionString("database");

var serviceProvider = builder.Build().Services;

FileSystemScriptOptions options = new()
{
    IncludeSubDirectories = false,
    Extensions = ["*.sql"],
    //Filter = (f) => !f.Contains(".local."),
    Encoding = Encoding.UTF8
};

Dictionary<string, string> variables = new()
{
    {"AI_CLIENT_ENDPOINT", ollamaEndpoint},
    //{"AI_CLIENT_KEY", Env.GetString("OPENAI_KEY")},
    {"EMBEDDING_DEPLOYMENT_NAME", embeddingModel},
    {"EMBEDDING_DIMENSIONS", embeddingDimensions.ToString("D", CultureInfo.InvariantCulture)}
};

logger.LogInformation("Starting deployment...");
var dbup = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithVariables(variables)
    .WithScriptsFromFileSystem(sqlFolder, options)
    .JournalToSqlTable("dbo", "$__dbup_journal")
    .AddLoggerFromServiceProvider(serviceProvider)
    .Build();

var result = dbup.PerformUpgrade();

if (!result.Successful)
{
    logger.LogError(result.Error, "Deployment failed.");
    return -1;
}

logger.LogInformation("Deployed successfully!");
return 0;
