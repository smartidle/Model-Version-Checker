using System.Text.Json.Serialization;

namespace ModelVersionChecker.Models.ApiResponse;

/// <summary>
/// Anthropic 模型列表响应格式
/// </summary>
public class AnthropicModelsResponse
{
    [JsonPropertyName("data")]
    public List<AnthropicModelItem> Data { get; set; } = new();

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("first_id")]
    public string? FirstId { get; set; }

    [JsonPropertyName("last_id")]
    public string? LastId { get; set; }
}

public class AnthropicModelItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
}
