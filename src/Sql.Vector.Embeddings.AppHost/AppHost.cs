var builder = DistributedApplication.CreateBuilder(args);

//builder.AddProject<Projects.Sql_Vector_Embeddings_BlobUploadConsole>("blob-upload-console");

await builder.Build().RunAsync();
