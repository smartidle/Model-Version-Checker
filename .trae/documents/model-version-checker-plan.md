# Model Version Checker — 实现计划

## 概述

使用 C# WPF (.NET 8) 构建一个桌面应用"Model Version Checker"，用户选择 AI 提供商、输入 API Key 后自动查询可用模型版本列表，支持导出 CSV/Excel。

---

## 当前状态分析

- 工作区 `d:\AiWorkplace\Model Version Checker` 为空目录
- 需要从零创建 WPF 项目

---

## 提供商 API 调研结果

### OpenAI 兼容格式（7家，通用一个实现类即可）

认证方式：`Authorization: Bearer <api_key>`
响应格式：`{"data": [{"id": "...", "object": "model", "created": ..., "owned_by": "..."}]}`

| 提供商 | Base URL | API Key 提示 |
|--------|----------|-------------|
| OpenAI | `https://api.openai.com/v1` | sk-... |
| DeepSeek | `https://api.deepseek.com/v1` | sk-... |
| Mistral | `https://api.mistral.ai/v1` | 无固定前缀 |
| Groq | `https://api.groq.com/openai/v1` | gsk_... |
| Kimi（月之暗面）| `https://api.moonshot.cn/v1` | sk-... |
| 智谱 GLM | `https://open.bigmodel.cn/api/paas/v4` | 无固定前缀 |
| 阿里百炼 | `https://dashscope.aliyuncs.com/compatible-mode/v1` | sk-... |

### 独立格式（4家，各自独立实现）

**Anthropic:**
- 端点：`GET https://api.anthropic.com/v1/models`
- 认证：`x-api-key: <key>` + `anthropic-version: 2023-06-01`
- 响应：`{"data": [{"id": "...", "type": "model", "display_name": "...", "created_at": "..."}]}`

**Google Gemini:**
- 端点：`GET https://generativelanguage.googleapis.com/v1beta/models?key=<api_key>`
- 认证：URL Query Parameter（非 Header）
- 响应：`{"models": [{"name": "models/gemini-...", "displayName": "...", "inputTokenLimit": ...}]}`

**百度千帆:**
- 端点：`GET https://qianfan.baidubce.com/v2/models`
- 认证：`Authorization: Bearer <api_key>`（新版兼容模式）
- 响应：OpenAI 兼容格式

**Cohere:**
- 端点：`GET https://api.cohere.com/v1/models`
- 认证：`Authorization: Bearer <api_key>`
- 响应：`{"models": [{"name": "...", "endpoints": [...], "context_length": ...}]}`

---

## 架构设计

### 技术栈
- **框架**: .NET 8 WPF
- **MVVM**: CommunityToolkit.Mvvm（源代码生成器，减少样板代码）
- **JSON**: System.Text.Json（内置）
- **CSV 导出**: CsvHelper
- **Excel 导出**: ClosedXML（MIT 协议）
- **HTTP**: HttpClient（单例管理）

### 项目结构

```
ModelVersionChecker/
├── ModelVersionChecker.sln
└── ModelVersionChecker/
    ├── ModelVersionChecker.csproj
    ├── App.xaml / App.xaml.cs
    ├── Models/
    │   ├── ModelInfo.cs                    # 统一模型数据
    │   ├── ProviderType.cs                 # 提供商枚举
    │   ├── ProviderDisplayItem.cs          # ComboBox 显示项
    │   └── ApiResponse/
    │       ├── OpenAIModelsResponse.cs
    │       ├── AnthropicModelsResponse.cs
    │       ├── GeminiModelsResponse.cs
    │       └── CohereModelsResponse.cs
    ├── Services/
    │   ├── IModelProviderService.cs        # 核心接口
    │   ├── OpenAICompatibleProviderService.cs  # 7家通用
    │   ├── AnthropicProviderService.cs
    │   ├── GeminiProviderService.cs
    │   ├── BaiduProviderService.cs
    │   ├── CohereProviderService.cs
    │   ├── ProviderServiceFactory.cs       # 工厂
    │   ├── IExportService.cs
    │   └── ExportService.cs                # CSV/Excel导出
    ├── ViewModels/
    │   ├── MainViewModel.cs
    │   └── ModelItemViewModel.cs
    ├── Views/
    │   ├── MainWindow.xaml
    │   └── MainWindow.xaml.cs
    ├── Converters/
    │   └── BooleanToVisibilityConverter.cs
    ├── Helpers/
    │   └── HttpClientSingleton.cs
    └── Resources/
        └── Styles.xaml
```

### 核心接口

```csharp
public interface IModelProviderService
{
    string ProviderName { get; }
    string ApiKeyHint { get; }
    Task<List<ModelInfo>> GetModelsAsync(string apiKey, CancellationToken ct = default);
}
```

### 统一数据模型

```csharp
public class ModelInfo
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Provider { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? OwnedBy { get; set; }
}
```

---

## UI 设计

```
┌──────────────────────────────────────────────────────┐
│  Model Version Checker                        _ □ X  │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ┌─ 配置 ──────────────────────────────────────────┐ │
│  │ AI 提供商:  [ OpenAI                    ▼ ]     │ │
│  │                                                  │ │
│  │ API Key:    [••••••••••••••••••••]  [👁]         │ │
│  │             提示: 请输入以 sk- 开头的 API Key      │ │
│  │                                                  │ │
│  │          [ 🔍 查询模型列表 ]                      │ │
│  └──────────────────────────────────────────────────┘ │
│                                                      │
│  ┌─ 查询结果 ──────────────────────────────────────┐ │
│  │ 找到 42 个模型                                   │ │
│  │ ┌──┬────────────┬──────┬──────────┬───────┐     │ │
│  │ │ #│ 模型 ID     │ 提供商│ 创建日期  │ 所有者│     │ │
│  │ ├──┼────────────┼──────┼──────────┼───────┤     │ │
│  │ │ 1│ gpt-4o     │OpenAI│2024-05-13│system │     │ │
│  │ │ 2│ gpt-4o-mini│OpenAI│2024-07-18│system │     │ │
│  │ └──┴────────────┴──────┴──────────┴───────┘     │ │
│  └──────────────────────────────────────────────────┘ │
│                                                      │
│  [📄 导出 CSV]  [📊 导出 Excel]  [📋 复制到剪贴板]   │
│                                                      │
│  状态: 就绪                                           │
└──────────────────────────────────────────────────────┘
```

---

## 实现步骤

### 步骤 1：项目初始化
- `dotnet new wpf -n ModelVersionChecker --framework net8.0`
- 添加 NuGet 包：CommunityToolkit.Mvvm、ClosedXML、CsvHelper
- 创建目录结构：Models/、Services/、ViewModels/、Views/、Converters/、Helpers/、Resources/

### 步骤 2：数据模型层（Models/）
- **ModelInfo.cs** — 统一模型数据（Id, DisplayName, Provider, CreatedDate, OwnedBy）
- **ProviderType.cs** — 提供商枚举（11个值）
- **ProviderDisplayItem.cs** — ComboBox 绑定用（ProviderType + Display Name）
- **ApiResponse/** — 4 组 DTO：OpenAI 格式、Anthropic 格式、Gemini 格式、Cohere 格式

### 步骤 3：服务层（Services/）
- **IModelProviderService.cs** — 核心接口（ProviderName, ApiKeyHint, GetModelsAsync）
- **OpenAICompatibleProviderService.cs** — 通用实现，通过构造函数传入 baseUrl/名称，覆盖 7 家
- **AnthropicProviderService.cs** — x-api-key Header + anthropic-version Header
- **GeminiProviderService.cs** — URL Query Parameter 认证
- **BaiduProviderService.cs** — 新版 v2 兼容接口
- **CohereProviderService.cs** — 独立响应格式 + 分页支持
- **ProviderServiceFactory.cs** — 工厂类，注册所有 11 家提供商的映射关系
- **ExportService.cs** — CSV（CsvHelper）+ Excel（ClosedXML）导出

### 步骤 4：ViewModel 层（ViewModels/）
- **MainViewModel.cs** — 使用 CommunityToolkit.Mvvm 的 [ObservableProperty] 和 [RelayCommand]
  - 属性：SelectedProvider, ApiKey, ApiKeyHint, Models, IsLoading, StatusMessage, HasResults, ResultCount
  - 命令：QueryCommand, ExportCsvCommand, ExportExcelCommand, CopyToClipboardCommand
  - 逻辑：验证输入 → 调用服务 → 更新 UI → 错误处理
- **ModelItemViewModel.cs** — DataGrid 行项（序号, 模型ID, 显示名称, 提供商, 创建日期, 所有者）

### 步骤 5：UI 层（Views/）
- **MainWindow.xaml** — 上方配置区（ComboBox + PasswordBox + 按钮），中间 DataGrid 结果区，下方导出按钮栏 + 状态栏
- **MainWindow.xaml.cs** — DataContext 绑定，PasswordBox 密码同步到 ViewModel
- **BooleanToVisibilityConverter.cs** — 控制加载动画、导出按钮可见性

### 步骤 6：辅助功能
- **HttpClientSingleton.cs** — 全局单例 HttpClient，30 秒超时
- **Styles.xaml** — 全局样式（按钮、DataGrid 样式美化）

### 步骤 7：整合测试
- 逐个提供商测试 API 调用
- 测试错误场景（无效 Key、网络断开、超时）
- 测试 CSV 和 Excel 导出

---

## 错误处理策略

| 错误 | 用户提示 |
|------|---------|
| 网络不可达 | "网络连接失败，请检查网络设置" |
| 401 | "API Key 无效，请检查后重试" |
| 403 | "无权访问，请确认 API Key 权限" |
| 429 | "请求过于频繁，请稍后再试" |
| 500 | "服务器内部错误，请稍后再试" |
| JSON 解析异常 | "响应格式异常，可能是 API 版本不兼容" |
| 超时/取消 | "查询已取消或超时" |

---

## API Key 安全措施
- 默认 PasswordBox 遮挡显示
- 提供显示/隐藏切换按钮
- 内存临时保存，不持久化到磁盘
- 日志和导出中自动脱敏

---

## 关键设计决策

1. **一个类覆盖 7 家提供商** — OpenAICompatibleProviderService 通过构造函数参数化 BaseURL，11 家提供商只需 5 个服务类
2. **工厂模式** — ProviderServiceFactory 管理所有提供商注册，新增提供商只需添加一行注册代码
3. **CommunityToolkit.Mvvm** — 使用源代码生成器，减少 INPC 样板代码
4. **ClosedXML 优于 EPPlus** — MIT 协议，无许可证限制
5. **桌面端无 CORS 限制** — 可直接调用所有 API

---

## 假设与约束

- 用户已安装 .NET 8 SDK
- 用户拥有各提供商的有效 API Key
- 应用仅用于本地查询，不涉及数据持久化
- 导出文件保存路径由用户通过对话框选择

---

## 验证步骤

1. 项目能成功编译（`dotnet build`）
2. 启动后能看到所有 11 家提供商的下拉选项
3. 选择提供商后 API Key 提示文本自动更新
4. 输入有效 API Key 后点击查询，能正确显示模型列表
5. 无效 API Key 给出友好错误提示
6. CSV 导出文件可用 Excel/记事本正确打开
7. Excel 导出文件格式正确（表头加粗、列宽自适应）
8. 复制到剪贴板功能正常工作
