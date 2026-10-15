var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var investmentOrdersDb = postgres.AddDatabase("investmentorders");

var cache = builder.AddRedis("cache").WithDataVolume();

var api = builder.AddDockerfile("api", "../..", "src/InvestmentOrders.Api/Dockerfile")
    .WithHttpEndpoint(port: 8080, targetPort: 8080)
    .WithExternalHttpEndpoints()
    .WithReference(investmentOrdersDb)
    .WithReference(cache)
    .WaitFor(investmentOrdersDb)
    .WaitFor(cache)
    // AddDockerfile resources, unlike AddProject, don't get OTEL_EXPORTER_OTLP_ENDPOINT injected automatically.
    .WithOtlpExporter();

builder.AddDockerfile("web", "../..", "web/slotbooking-web/Dockerfile")
    .WithHttpEndpoint(port: 4201, targetPort: 80)
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
