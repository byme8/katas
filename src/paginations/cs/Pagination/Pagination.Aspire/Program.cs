var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongodb")
    .WithDataVolume()
    .WithEndpoint(27017, 27017, name: "mongodb")
    .AddDatabase("paginationdb");

builder.AddProject<Projects.Pagination>("pagination")
    .WithReference(mongo)
    .WaitFor(mongo);

builder.Build().Run();