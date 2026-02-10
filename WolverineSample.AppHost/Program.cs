var builder = DistributedApplication.CreateBuilder(args);

var postgresUserName = builder.AddParameter("postgresUsername", "postgres");
var postgresPassword = builder.AddParameter("postgresPassword", "postgres", secret: true);

var rabbitMqUserName = builder.AddParameter("rabbitmqUsername", "guest");
var rabbitMqPassword = builder.AddParameter("rabbitmqPassword", "guest", secret: true);
var postgresModuleA = builder.AddPostgres("module-a-postgres", postgresUserName, postgresPassword, 5433);
var postgresModuleB = builder.AddPostgres("module-b-postgres", postgresUserName, postgresPassword, 5434);
var mainPostgres = builder.AddPostgres("main-postgres", postgresUserName, postgresPassword, 5435);
var rabbitMq = builder.AddRabbitMQ("rabbitMq", rabbitMqUserName, rabbitMqPassword, 5672);

var moduleA = builder.AddProject<Projects.WolverineSample_API>("moduleA")
    .WithReference(postgresModuleA)
    .WithReference(postgresModuleB)
    .WithReference(mainPostgres)
    .WithReference(rabbitMq)
    .WithHttpHealthCheck("/health");

builder.Build().Run();