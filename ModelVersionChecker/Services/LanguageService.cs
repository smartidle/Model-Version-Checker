using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelVersionChecker.Models;

namespace ModelVersionChecker.Services;

/// <summary>
/// 支持的语言
/// </summary>
public enum AppLanguage
{
    English,
    Chinese
}

/// <summary>
/// 集中式多语言服务，所有 UI 文本通过此类获取
/// </summary>
public class LanguageService : INotifyPropertyChanged
{
    private AppLanguage _current = AppLanguage.English;

    public AppLanguage Current
    {
        get => _current;
        set
        {
            if (_current != value)
            {
                _current = value;
                OnPropertyChanged(null); // 通知所有属性变更
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private string En(string en, string zh) => _current == AppLanguage.English ? en : zh;

    // === 窗口标题 ===
    public string AppTitle => En("Model Version Checker", "Model Version Checker - AI 模型版本查询器");

    // === 配置区域 ===
    public string ConfigHeader => En("⚙️ Configuration", "⚙️ 配置");
    public string ProviderLabel => En("AI Provider:", "AI 提供商:");
    public string ApiKeyLabel => En("API Key:", "API Key:");
    public string QueryButton => En("🔍 Query Models", "🔍 查询模型列表");
    public string QueryingButton => En("⏳ Querying...", "⏳ 查询中...");

    // === 结果区域 ===
    public string ResultHeader => En("📋 Query Results", "📋 查询结果");
    public string FoundText => En("Found ", "找到 ");
    public string ModelsText => En(" models", " 个模型");
    public string LoadingText => En("⏳ Querying, please wait...", "⏳ 正在查询，请稍候...");
    public string EmptyStateText => En("Select a provider, enter API Key, then click Query", "请选择提供商并输入 API Key 后点击查询");

    // === DataGrid 列头 ===
    public string ColModelId => En("Model ID", "模型 ID");
    public string ColDisplayName => En("Display Name", "显示名称");
    public string ColProvider => En("Provider", "提供商");
    public string ColCreatedDate => En("Created Date", "创建日期");
    public string ColOwner => En("Owner", "所有者");

    // === 导出按钮 ===
    public string ExportCsvButton => En("📄 Export CSV", "📄 导出 CSV");
    public string ExportExcelButton => En("📊 Export Excel", "📊 导出 Excel");
    public string CopyClipboardButton => En("📋 Copy to Clipboard", "📋 复制到剪贴板");

    // === 状态消息 ===
    public string StatusReady => En("Ready - Select an AI provider and enter API Key", "就绪 - 请选择 AI 提供商并输入 API Key");
    public string StatusSelectProvider => En("Please select an AI provider first", "请先选择 AI 提供商");
    public string StatusEnterApiKey => En("Please enter API Key", "请输入 API Key");
    public string StatusNoProviderSelected => En("Please select a provider first", "请先选择 AI 提供商");

    public string StatusQuerying(string provider) =>
        En($"Querying available models from {provider}...", $"正在查询 {provider} 的可用模型...");
    public string StatusSelected(string provider) =>
        En($"Selected: {provider} - Enter API Key and click Query", $"已选择: {provider} - 请输入 API Key 后点击查询");
    public string StatusSuccess(int count) =>
        En($"✅ Query completed - Found {count} available models", $"✅ 查询完成 - 共找到 {count} 个可用模型");
    public string StatusNoModels =>
        En("⚠️ Query completed, but no models found", "⚠️ 查询完成，但未找到可用模型");
    public string StatusCancelled =>
        En("❌ Query timed out or cancelled, please check network", "❌ 查询超时或已取消，请检查网络连接后重试");

    // === 错误消息 ===
    public string ErrUnauthorized => En("Invalid API Key, please check and retry", "API Key 无效，请检查后重试");
    public string ErrForbidden => En("Access denied, please verify API Key permissions", "无权访问，请确认 API Key 权限");
    public string ErrTooManyRequests => En("Too many requests, please try again later", "请求过于频繁，请稍后再试");
    public string ErrNetwork(string msg) => En($"Network error: {msg}", $"网络请求失败: {msg}");
    public string ErrQueryFailed(string msg) => En($"Query failed: {msg}", $"❌ 查询失败: {msg}");

    // === 导出消息 ===
    public string CsvExported(string path) => En($"✅ CSV exported: {path}", $"✅ CSV 已导出: {path}");
    public string ExcelExported(string path) => En($"✅ Excel exported: {path}", $"✅ Excel 已导出: {path}");
    public string ExportFailed(string msg) => En($"Export failed: {msg}", $"❌ 导出失败: {msg}");
    public string Copied(int count) => En($"✅ Copied {count} model records to clipboard", $"✅ 已复制 {count} 条模型记录到剪贴板");
    public string CopyFailed(string msg) => En($"Copy failed: {msg}", $"❌ 复制失败: {msg}");

    // === 导出对话框 ===
    public string CsvFilter => En("CSV Files (*.csv)|*.csv", "CSV 文件 (*.csv)|*.csv");
    public string ExcelFilter => En("Excel Files (*.xlsx)|*.xlsx", "Excel 文件 (*.xlsx)|*.xlsx");
    public string CsvFileName(string provider) => En($"Models_{provider}_{DateTime.Now:yyyyMMdd_HHmmss}", $"模型列表_{provider}_{DateTime.Now:yyyyMMdd_HHmmss}");
    public string ExcelFileName(string provider) => En($"Models_{provider}_{DateTime.Now:yyyyMMdd_HHmmss}", $"模型列表_{provider}_{DateTime.Now:yyyyMMdd_HHmmss}");

    // === 导出表头 ===
    public string HeaderIndex => En("#", "序号");
    public string HeaderModelId => En("Model ID", "模型ID");
    public string HeaderDisplayName => En("Display Name", "显示名称");
    public string HeaderProvider => En("Provider", "提供商");
    public string HeaderCreatedDate => En("Created Date", "创建日期");
    public string HeaderOwner => En("Owner", "所有者");

    // === 提供商名称（英文模式下用英文名） ===
    public string GetProviderDisplayName(ProviderType type) => type switch
    {
        ProviderType.OpenAI => "OpenAI",
        ProviderType.Anthropic => "Anthropic (Claude)",
        ProviderType.GoogleGemini => En("Google Gemini", "Google Gemini"),
        ProviderType.DeepSeek => "DeepSeek",
        ProviderType.Mistral => "Mistral",
        ProviderType.Groq => "Groq",
        ProviderType.MoonshotKimi => En("Moonshot (Kimi)", "Kimi (月之暗面)"),
        ProviderType.ZhipuGLM => En("Zhipu GLM", "智谱 GLM"),
        ProviderType.AliDashScope => En("Alibaba DashScope (Qwen)", "阿里百炼 (通义千问)"),
        ProviderType.BaiduQianfan => En("Baidu Qianfan (ERNIE)", "百度千帆 (文心)"),
        ProviderType.Cohere => "Cohere",
        _ => type.ToString()
    };

    public string GetProviderApiKeyHint(ProviderType type) => type switch
    {
        ProviderType.OpenAI => En("Enter API Key (sk-...)", "请输入 API Key (sk-...)"),
        ProviderType.Anthropic => En("Enter API Key (sk-ant-...)", "请输入 API Key (sk-ant-...)"),
        ProviderType.GoogleGemini => En("Enter API Key", "请输入 API Key"),
        ProviderType.DeepSeek => En("Enter API Key", "请输入 API Key"),
        ProviderType.Mistral => En("Enter API Key", "请输入 API Key"),
        ProviderType.Groq => En("Enter API Key (gsk_...)", "请输入 API Key (gsk_...)"),
        ProviderType.MoonshotKimi => En("Enter API Key (sk-...)", "请输入 API Key (sk-...)"),
        ProviderType.ZhipuGLM => En("Enter API Key", "请输入 API Key"),
        ProviderType.AliDashScope => En("Enter API Key (sk-...)", "请输入 API Key (sk-...)"),
        ProviderType.BaiduQianfan => En("Enter API Key (from Baidu Cloud Console)", "请输入 API Key (百度智能云控制台获取)"),
        ProviderType.Cohere => En("Enter API Key", "请输入 API Key"),
        _ => En("Enter API Key", "请输入 API Key")
    };

    // === 语言选择 ===
    public string LanguageLabel => En("Language:", "语言:");
}
