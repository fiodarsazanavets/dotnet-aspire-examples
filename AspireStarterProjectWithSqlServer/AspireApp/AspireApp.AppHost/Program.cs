var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);
var sql = builder.AddSqlServer("sql", password);
var sqldb = sql.AddDatabase("sqldb");

var apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(sqldb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
