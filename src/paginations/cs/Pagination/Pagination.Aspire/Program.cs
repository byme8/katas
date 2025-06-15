var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: password)
    .WithHostPort(5432)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("paginationdb");

builder.AddProject<Projects.Pagination>("pagination")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();