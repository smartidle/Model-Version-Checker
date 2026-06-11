namespace ModelVersionChecker.Models;

/// <summary>
/// 统一的模型信息数据模型，所有提供商的查询结果都映射到此格式
/// </summary>
public class ModelInfo
{
    /// <summary>模型唯一标识（如 gpt-4o, claude-3-opus）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>模型显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>所属提供商名称</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>模型创建日期</summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>模型所有者</summary>
    public string? OwnedBy { get; set; }
}
