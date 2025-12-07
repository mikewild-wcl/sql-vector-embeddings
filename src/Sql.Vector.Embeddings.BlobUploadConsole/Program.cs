using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sql.Vector.Embeddings.BlobUploadConsole.Services;

//var builder = Host.CreateDefaultBuilder()
//    .ConfigureServices((hostContext, services) =>
//    {
//        services.AddHostedService<BlobUploadService>();

//        services.AddAzureClients(cb =>
//        {
//            var blobStorageConnectionString = hostContext.Configuration["blobStorage:connectionString"];
//            cb.AddBlobServiceClient(blobStorageConnectionString);
//        });
//    });

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

builder.Services.AddHostedService<BlobUploadService>();
builder.Services.AddAzureClients(cb =>
    {
        var blobStorageConnectionString = builder.Configuration["blobStorage:connectionString"];
        if( string.IsNullOrEmpty(blobStorageConnectionString))
        {
            throw new InvalidOperationException("Configuration for blobStorage:connectionString not found");
        }
        cb.AddBlobServiceClient(blobStorageConnectionString);
    });

var app = builder.Build();

await app.RunAsync();