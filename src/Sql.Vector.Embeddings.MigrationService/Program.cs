using Sql.Vector.Embeddings.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

//builder.AddSqlServerDbContext<TicketContext>("sqldata");
builder.AddSqlServerClient("sql");

var host = builder.Build();

await host.RunAsync();

return 0;