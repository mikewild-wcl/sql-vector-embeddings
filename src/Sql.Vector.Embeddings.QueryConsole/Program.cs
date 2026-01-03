using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

builder.AddOllamaApiClient("chat").AddChatClient();
builder.AddOllamaApiClient("embeddings").AddEmbeddingGenerator();

// Interactive console from AppHost
//      https://rasper87.blog/2025/10/29/building-a-dynamic-command-sample-with-net-aspire/
//      https://github.com/dotnet/aspire/discussions/4625

var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();

var agent = chatClient
    .CreateAIAgent(
        instructions: "You are a helpful assistant that loves talking about yourself.",
        name: "Assistant");
var thread = agent.GetNewThread();

if (args.Length > 0)
{
    Console.WriteLine("args:");    
    foreach (var a in args)
    {
        Console.WriteLine($"    {a}");
    }
    Console.WriteLine();
}

await foreach (var update in agent.RunStreamingAsync("Introduce yourself.", thread))
{
    Console.Write(update);
}
Console.WriteLine();

string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    Console.Write(@"Assistant > ");
    await foreach (var update in agent.RunStreamingAsync(userInput, thread))
    {
        Console.Write(update);
    }
    Console.WriteLine();
} while (userInput is { Length: > 0 });
