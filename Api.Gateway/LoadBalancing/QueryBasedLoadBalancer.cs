using Ocelot.LoadBalancer.Interfaces;
using Ocelot.Responses;
using Ocelot.Values;

namespace Api.Gateway.LoadBalancing;

/// <summary>
/// Балансировщик нагрузки на основе параметра запроса.
/// Реплика определяется как остаток от деления id на число реплик: index = id % N.
/// </summary>
public class QueryBasedLoadBalancer(Func<Task<List<Service>>> services) : ILoadBalancer
{
    public string Type => nameof(QueryBasedLoadBalancer);

    public async Task<Response<ServiceHostAndPort>> LeaseAsync(HttpContext httpContext)
    {
        var list = await services();

        if (list.Count == 0)
            throw new InvalidOperationException("No available downstream services.");

        var query = httpContext.Request.Query;

        if (!query.ContainsKey("id") || !int.TryParse(query["id"], out var id))
        {
            return new OkResponse<ServiceHostAndPort>(list[0].HostAndPort);
        }

        var index = Math.Abs(id) % list.Count;
        return new OkResponse<ServiceHostAndPort>(list[index].HostAndPort);
    }

    public void Release(ServiceHostAndPort hostAndPort) { }
}
