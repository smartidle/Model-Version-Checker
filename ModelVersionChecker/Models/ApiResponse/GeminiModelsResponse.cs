using System.Text.Json.Serialization;

namespace ModelVersionChecker.Models.ApiResponse;

/// <summary>
/// Google Gemini 模型列表响应格式
/// </summary>
public class GeminiModelsResponse
{
    [JsonPropertyName("models")]
    public List<GeminiModelItem> Models { get; set; } = new();
}

public class GeminiModelItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("supportedGenerationMethods")]
    public List<string>? SupportedGenerationMethods { get; set; }

    [JsonPropertyName("inputTokenLimit")]
    public int? InputTokenLimit { get; set; }

    [JsonPropertyName("outputTokenLimit")]
    public int? OutputTokenLimit { get; set; }
}
