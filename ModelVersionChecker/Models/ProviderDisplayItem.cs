namespace ModelVersionChecker.Models;

/// <summary>
/// ComboBox 显示项，用于绑定提供商列表
/// </summary>
public class ProviderDisplayItem
{
    /// <summary>提供商类型</summary>
    public ProviderType Type { get; }

    /// <summary>显示名称</summary>
    public string DisplayName { get; }

    /// <summary>API Key 输入提示</summary>
    public string ApiKeyHint { get; }

    public ProviderDisplayItem(ProviderType type, string displayName, string apiKeyHint)
    {
        Type = type;
        DisplayName = displayName;
        ApiKeyHint = apiKeyHint;
    }

    public override string ToString() => DisplayName;
}
