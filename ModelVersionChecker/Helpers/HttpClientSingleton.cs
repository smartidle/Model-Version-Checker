using System.Net.Http;

namespace ModelVersionChecker.Helpers;

/// <summary>
/// HttpClient 全局单例管理
/// </summary>
public static class HttpClientSingleton
{
    private static readonly Lazy<HttpClient> _instance = new(() =>
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    });

    public static HttpClient Instance => _instance.Value;
}
