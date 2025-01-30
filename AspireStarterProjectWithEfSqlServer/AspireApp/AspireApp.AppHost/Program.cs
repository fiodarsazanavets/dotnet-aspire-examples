IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> password = builder.AddParameter("sql-password", secret: true);
IResourceBuilder<SqlServerServerResource> sql = builder.AddSqlServer("sql", password);
IResourceBuilder<SqlServerDatabaseResource> sqldb = sql.AddDatabase("sqldb");

IResourceBuilder<ProjectResource> apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(sqldb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
