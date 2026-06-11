using System.Net.Http;
using ModelVersionChecker.Models;

namespace ModelVersionChecker.Services;

/// <summary>
/// 提供商服务工厂，管理所有提供商的注册和创建
/// </summary>
public class ProviderServiceFactory
{
    private readonly Dictionary<ProviderType, Func<IModelProviderService>> _factories;
    private readonly HttpClient _httpClient;

    public ProviderServiceFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _factories = new Dictionary<ProviderType, Func<IModelProviderService>>
        {
            // OpenAI 兼容格式提供商（7家共用一个实现类）
            [ProviderType.OpenAI] = () => new OpenAICompatibleProviderService(
                "https://api.openai.com/v1", "OpenAI", "请输入 API Key (sk-...)", _httpClient),

            [ProviderType.DeepSeek] = () => new OpenAICompatibleProviderService(
                "https://api.deepseek.com/v1", "DeepSeek", "请输入 API Key", _httpClient),

            [ProviderType.Mistral] = () => new OpenAICompatibleProviderService(
                "https://api.mistral.ai/v1", "Mistral", "请输入 API Key", _httpClient),

            [ProviderType.Groq] = () => new OpenAICompatibleProviderService(
                "https://api.groq.com/openai/v1", "Groq", "请输入 API Key (gsk_...)", _httpClient),

            [ProviderType.MoonshotKimi] = () => new OpenAICompatibleProviderService(
                "https://api.moonshot.cn/v1", "Kimi (月之暗面)", "请输入 API Key (sk-...)", _httpClient),

            [ProviderType.ZhipuGLM] = () => new OpenAICompatibleProviderService(
                "https://open.bigmodel.cn/api/paas/v4", "智谱 GLM", "请输入 API Key", _httpClient),

            [ProviderType.AliDashScope] = () => new OpenAICompatibleProviderService(
                "https://dashscope.aliyuncs.com/compatible-mode/v1", "阿里百炼 (通义千问)", "请输入 API Key (sk-...)", _httpClient),

            // 独立格式提供商
            [ProviderType.Anthropic] = () => new AnthropicProviderService(_httpClient),

            [ProviderType.GoogleGemini] = () => new GeminiProviderService(_httpClient),

            [ProviderType.BaiduQianfan] = () => new BaiduProviderService(_httpClient),

            [ProviderType.Cohere] = () => new CohereProviderService(_httpClient),
        };
    }

    /// <summary>
    /// 根据提供商类型创建对应的服务实例
    /// </summary>
    public IModelProviderService Create(ProviderType providerType)
    {
        if (_factories.TryGetValue(providerType, out var factory))
        {
            return factory();
        }
        throw new NotSupportedException($"不支持的提供商类型: {providerType}");
    }

    /// <summary>
    /// 获取所有已注册的提供商显示列表
    /// </summary>
    public List<ProviderDisplayItem> GetAllProviders()
    {
        var providers = new List<ProviderDisplayItem>();

        foreach (var kvp in _factories)
        {
            var service = kvp.Value();
            providers.Add(new ProviderDisplayItem(kvp.Key, service.ProviderName, service.ApiKeyHint));
        }

        return providers;
    }
}
