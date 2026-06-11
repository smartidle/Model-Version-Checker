using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ModelVersionChecker.Models;
using ModelVersionChecker.Models.ApiResponse;

namespace ModelVersionChecker.Services;

/// <summary>
/// Cohere 提供商服务
/// 认证方式: Authorization: Bearer {api_key}
/// 支持分页获取所有模型
/// </summary>
public class CohereProviderService : IModelProviderService
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "Cohere";
    public string ApiKeyHint => "请输入 API Key";

    public CohereProviderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var allModels = new List<ModelInfo>();
        string? nextPageToken = null;

        do
        {
            var url = "https://api.cohere.com/v1/models";
            if (!string.IsNullOrEmpty(nextPageToken))
            {
                url += $"?page_token={Uri.EscapeDataString(nextPageToken)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<CohereModelsResponse>(json, JsonOptions.Default)
                ?? throw new InvalidOperationException("无法解析 API 响应");

            foreach (var item in result.Models)
            {
                allModels.Add(new ModelInfo
                {
                    Id = item.Name,
                    DisplayName = item.Name,
                    Provider = ProviderName,
                    OwnedBy = "cohere"
                });
            }

            nextPageToken = result.NextPageToken;
        }
        while (!string.IsNullOrEmpty(nextPageToken));

        return allModels.OrderBy(m => m.Id).ToList();
    }
}
