using Microsoft.Extensions.Hosting;
using OllamaSharp;
using System.Net.Http;
using System.Threading;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

builder.AddOllamaApiClient("chat");
builder.AddOllamaApiClient("embeddings");

//TODO: can I get "AISettings:embeddingDimensions" from the embedding model?

/*
var agent = new AzureOpenAIClient(
    new Uri(configuration["AzureOpenAiSettings:Endpoint"]!),
    new ApiKeyCredential(configuration["AzureOpenAiSettings:ApiKey"]!),
    new AzureOpenAIClientOptions
    {
        Transport = new HttpClientPipelineTransport(httpClient)
    })
    .GetChatClient(configuration["AzureOpenAiSettings:DeploymentName"])
    .CreateAIAgent(
        instructions: "You are a helpful assistant that loves talking about cooking.",
        name: "Assistant"
        );

var thread = agent.GetNewThread();
*/

string? userInput;
do
{
    Console.Write("""User > """);
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    Console.Write(@"Assistant > ");
    //await foreach (var update in agent.RunStreamingAsync(userInput, thread))
    //{
    //    Console.Write(update);
    //}
    Console.WriteLine();
} while (userInput is { Length: > 0 });
