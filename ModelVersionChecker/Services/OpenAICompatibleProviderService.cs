using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ModelVersionChecker.Models;
using ModelVersionChecker.Models.ApiResponse;

namespace ModelVersionChecker.Services;

/// <summary>
/// OpenAI 兼容格式的提供商通用实现
/// 适用于: OpenAI, DeepSeek, Mistral, Groq, Kimi, 智谱GLM, 阿里百炼
/// </summary>
public class OpenAICompatibleProviderService : IModelProviderService
{
    private readonly string _baseUrl;
    private readonly string _providerName;
    private readonly string _apiKeyHint;
    private readonly HttpClient _httpClient;

    public string ProviderName => _providerName;
    public string ApiKeyHint => _apiKeyHint;

    public OpenAICompatibleProviderService(string baseUrl, string providerName, string apiKeyHint, HttpClient httpClient)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _providerName = providerName;
        _apiKeyHint = apiKeyHint;
        _httpClient = httpClient;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/models";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<OpenAIModelsResponse>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException("无法解析 API 响应");

        return result.Data.Select(item => new ModelInfo
        {
            Id = item.Id,
            DisplayName = item.Id,
            Provider = _providerName,
            CreatedDate = item.Created.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(item.Created.Value).DateTime
                : null,
            OwnedBy = item.OwnedBy
        }).OrderBy(m => m.Id).ToList();
    }
}

/// <summary>
/// 全局 JSON 序列化选项
/// </summary>
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
