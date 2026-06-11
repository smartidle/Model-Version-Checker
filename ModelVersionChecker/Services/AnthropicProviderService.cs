using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ModelVersionChecker.Models;
using ModelVersionChecker.Models.ApiResponse;

namespace ModelVersionChecker.Services;

/// <summary>
/// Anthropic (Claude) 提供商服务
/// 认证方式: x-api-key Header + anthropic-version Header
/// </summary>
public class AnthropicProviderService : IModelProviderService
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "Anthropic";
    public string ApiKeyHint => "请输入 API Key (sk-ant-...)";

    public AnthropicProviderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        const string url = "https://api.anthropic.com/v1/models";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<AnthropicModelsResponse>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException("无法解析 API 响应");

        return result.Data.Select(item => new ModelInfo
        {
            Id = item.Id,
            DisplayName = item.DisplayName ?? item.Id,
            Provider = ProviderName,
            CreatedDate = ParseAnthropicDate(item.CreatedAt),
            OwnedBy = "anthropic"
        }).OrderBy(m => m.Id).ToList();
    }

    private static DateTime? ParseAnthropicDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString)) return null;
        return DateTime.TryParse(dateString, out var date) ? date : null;
    }
}
