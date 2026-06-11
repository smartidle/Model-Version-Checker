using System.Text.Json.Serialization;

namespace ModelVersionChecker.Models.ApiResponse;

/// <summary>
/// Cohere 模型列表响应格式
/// </summary>
public class CohereModelsResponse
{
    [JsonPropertyName("models")]
    public List<CohereModelItem> Models { get; set; } = new();

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

public class CohereModelItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_deprecated")]
    public bool? IsDeprecated { get; set; }

    [JsonPropertyName("endpoints")]
    public List<string>? Endpoints { get; set; }

    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }

    [JsonPropertyName("default_endpoints")]
    public List<string>? DefaultEndpoints { get; set; }
}
