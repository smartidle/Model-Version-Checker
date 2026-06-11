using System.Net.Http;
using System.Text.Json;
using ModelVersionChecker.Models;
using ModelVersionChecker.Models.ApiResponse;

namespace ModelVersionChecker.Services;

/// <summary>
/// Google Gemini 提供商服务
/// 认证方式: URL Query Parameter (key=)
/// </summary>
public class GeminiProviderService : IModelProviderService
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "Google Gemini";
    public string ApiKeyHint => "请输入 API Key";

    public GeminiProviderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={Uri.EscapeDataString(apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeminiModelsResponse>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException("无法解析 API 响应");

        return result.Models.Select(item =>
        {
            // name 格式为 "models/gemini-pro"，提取实际模型名
            var modelId = item.Name.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
                ? item.Name["models/".Length..]
                : item.Name;

            return new ModelInfo
            {
                Id = modelId,
                DisplayName = item.DisplayName ?? modelId,
                Provider = ProviderName,
                OwnedBy = "google"
            };
        }).OrderBy(m => m.Id).ToList();
    }
}
