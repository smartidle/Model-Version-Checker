using ModelVersionChecker.Models;

namespace ModelVersionChecker.Services;

/// <summary>
/// AI 模型提供商服务接口，所有提供商必须实现此接口
/// </summary>
public interface IModelProviderService
{
    /// <summary>提供商显示名称</summary>
    string ProviderName { get; }

    /// <summary>API Key 输入提示文本</summary>
    string ApiKeyHint { get; }

    /// <summary>
    /// 查询可用模型列表
    /// </summary>
    /// <param name="apiKey">API 授权码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken cancellationToken = default);
}
