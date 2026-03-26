var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

for (var i = 0; i < 3; i++)
{
    var service = builder.AddProject<Projects.CreditApp_Api>($"credit-app-{i}", launchProfileName: null)
        .WithReference(redis)
        .WaitFor(redis)
        .WithHttpsEndpoint(port: 8000 + i);
    gateway.WithReference(service).WaitFor(service);
}

builder.AddProject<Projects.Client_Wasm>("client-wasm")
    .WaitFor(gateway);

builder.Build().Run();
