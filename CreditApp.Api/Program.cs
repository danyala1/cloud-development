using CreditApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("redis");

builder.Services.AddSingleton<CreditApplicationGenerator>();
builder.Services.AddScoped<ICreditApplicationService, CreditApplicationService>();

var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("AllowClient", policy =>
    policy.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()));

var app = builder.Build();

app.UseCors("AllowClient");
app.MapDefaultEndpoints();

app.MapGet("/credit-application", async (int id, ICreditApplicationService service) =>
    await service.GetOrGenerate(id));

app.Run();
