using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ModelVersionChecker.Models;
using ModelVersionChecker.Models.ApiResponse;

namespace ModelVersionChecker.Services;

/// <summary>
/// 百度千帆提供商服务（使用新版 v2 OpenAI 兼容模式）
/// 认证方式: Authorization: Bearer {api_key}
/// </summary>
public class BaiduProviderService : IModelProviderService
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "百度千帆";
    public string ApiKeyHint => "请输入 API Key (百度智能云控制台获取)";

    public BaiduProviderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        const string url = "https://qianfan.baidubce.com/v2/models";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        // 百度千帆 v2 返回 OpenAI 兼容格式
        var result = JsonSerializer.Deserialize<OpenAIModelsResponse>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException("无法解析 API 响应");

        return result.Data.Select(item => new ModelInfo
        {
            Id = item.Id,
            DisplayName = item.Id,
            Provider = ProviderName,
            CreatedDate = item.Created.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(item.Created.Value).DateTime
                : null,
            OwnedBy = item.OwnedBy ?? "baidu"
        }).OrderBy(m => m.Id).ToList();
    }
}
