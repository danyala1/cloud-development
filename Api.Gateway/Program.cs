using Api.Gateway.LoadBalancing;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceDiscovery();
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var overrides = new Dictionary<string, string?>();
for (var i = 0; Environment.GetEnvironmentVariable($"services__credit-app-{i}__https__0") is { } url; i++)
{
    var uri = new Uri(url);
    overrides[$"Routes:0:DownstreamHostAndPorts:{i}:Host"] = uri.Host;
    overrides[$"Routes:0:DownstreamHostAndPorts:{i}:Port"] = uri.Port.ToString();
}

if (overrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(overrides);

builder.Services.AddOcelot()
    .AddCustomLoadBalancer<QueryBasedLoadBalancer>((_, _, discoveryProvider) => new(discoveryProvider.GetAsync));

var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("AllowClient", policy =>
    policy.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()));

var app = builder.Build();

app.UseCors("AllowClient");
app.MapDefaultEndpoints();

await app.UseOcelot();

app.Run();
