using System.Text.Json;
using CreditApp.Api.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace CreditApp.Api.Services;

/// <summary>
/// Реализация сервиса кредитных заявок с кэшированием в Redis
/// </summary>
public class CreditApplicationService(
    IDistributedCache cache,
    CreditApplicationGenerator generator,
    IConfiguration configuration,
    ILogger<CreditApplicationService> logger) : ICreditApplicationService
{
    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
            configuration.GetValue<int>("CacheSettings:ExpirationMinutes"))
    };

    /// <inheritdoc />
    public async Task<CreditApplication> GetOrGenerate(int id)
    {
        var key = $"credit-application:{id}";

        logger.LogInformation("Processing request for credit application {Id}", id);

        try
        {
            var cached = await cache.GetStringAsync(key);
            if (cached is not null)
            {
                logger.LogInformation("Cache hit for credit application {Id}", id);
                return JsonSerializer.Deserialize<CreditApplication>(cached)!;
            }

            logger.LogInformation("Cache miss for credit application {Id}", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading from cache for credit application {Id}", id);
        }

        try
        {
            var application = generator.Generate(id);

            logger.LogInformation(
                "Generated credit application {Id}, status: {Status}, amount: {RequestedAmount}",
                id, application.Status, application.RequestedAmount);

            try
            {
                var json = JsonSerializer.Serialize(application);
                await cache.SetStringAsync(key, json, _cacheOptions);
                logger.LogInformation("Cached credit application {Id}", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error writing to cache for credit application {Id}", id);
            }

            return application;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating credit application {Id}", id);
            throw;
        }
    }
}
