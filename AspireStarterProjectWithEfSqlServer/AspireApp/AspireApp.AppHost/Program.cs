var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);
var sql = builder.AddSqlServer("sql", password).WithLifetime(ContainerLifetime.Persistent);
var sqldb = sql.AddDatabase("sqldb");

var apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(sqldb)
    .WithReference(sqldb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
