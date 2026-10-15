// Orchestrates the whole system: PostgreSQL, the .NET API, and the Angular web app.
// Run this project (F5 in Visual Studio/Rider, or ./run.sh) and the Aspire dashboard opens.
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var database = postgres.AddDatabase("slotbookingdb");

var api = builder.AddProject<Projects.SlotBooking_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database);

builder.AddJavaScriptApp("web", "../../web/slotbooking-web", runScriptName: "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
