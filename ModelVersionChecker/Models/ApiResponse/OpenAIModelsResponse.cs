using System.Text.Json.Serialization;

namespace ModelVersionChecker.Models.ApiResponse;

/// <summary>
/// OpenAI 兼容格式的模型列表响应
/// 适用于: OpenAI, DeepSeek, Mistral, Groq, Kimi, 智谱GLM, 阿里百炼
/// </summary>
public class OpenAIModelsResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("data")]
    public List<OpenAIModelItem> Data { get; set; } = new();
}

public class OpenAIModelItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }

    [JsonPropertyName("owned_by")]
    public string? OwnedBy { get; set; }
}
