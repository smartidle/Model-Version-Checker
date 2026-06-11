using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModelVersionChecker.Models;
using ModelVersionChecker.Services;

namespace ModelVersionChecker.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ProviderServiceFactory _factory;
    private readonly IExportService _exportService;
    private readonly LanguageService _lang;
    private CancellationTokenSource? _cts;

    // === 语言相关属性 ===

    [ObservableProperty]
    private AppLanguage _selectedLanguage = AppLanguage.English;

    [ObservableProperty]
    private ObservableCollection<string> _availableLanguages = new() { "English", "中文" };

    [ObservableProperty]
    private int _selectedLanguageIndex = 0;

    // === 数据属性 ===

    [ObservableProperty]
    private ObservableCollection<ProviderDisplayItem> _providers = new();

    [ObservableProperty]
    private ProviderDisplayItem? _selectedProvider;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _apiKeyHint = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ModelItemViewModel> _models = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private int _resultCount;

    [ObservableProperty]
    private string _queryButtonText = string.Empty;

    // === 绑定到 Lang 的本地化文本 ===

    public string AppTitle => _lang.AppTitle;
    public string ConfigHeader => _lang.ConfigHeader;
    public string ProviderLabel => _lang.ProviderLabel;
    public string ApiKeyLabel => _lang.ApiKeyLabel;
    public string ResultHeader => _lang.ResultHeader;
    public string FoundText => _lang.FoundText;
    public string ModelsText => _lang.ModelsText;
    public string LoadingText => _lang.LoadingText;
    public string EmptyStateText => _lang.EmptyStateText;
    public string ColModelId => _lang.ColModelId;
    public string ColDisplayName => _lang.ColDisplayName;
    public string ColProvider => _lang.ColProvider;
    public string ColCreatedDate => _lang.ColCreatedDate;
    public string ColOwner => _lang.ColOwner;
    public string ExportCsvButton => _lang.ExportCsvButton;
    public string ExportExcelButton => _lang.ExportExcelButton;
    public string CopyClipboardButton => _lang.CopyClipboardButton;
    public string LanguageLabel => _lang.LanguageLabel;

    public MainViewModel(ProviderServiceFactory factory, IExportService exportService, LanguageService lang)
    {
        _factory = factory;
        _exportService = exportService;
        _lang = lang;

        // 监听语言服务属性变更，转发通知
        _lang.PropertyChanged += (s, e) => OnPropertyChanged(string.Empty);

        // 加载提供商列表
        RefreshProviders();

        // 初始化 UI 文本
        ApplyLanguage();
    }

    // === 语言切换回调 ===
    partial void OnSelectedLanguageIndexChanged(int value)
    {
        _lang.Current = value == 1 ? AppLanguage.Chinese : AppLanguage.English;
        SelectedLanguage = _lang.Current;

        // 刷新提供商列表（名称变化）
        RefreshProviders();

        // 刷新所有 UI 文本
        ApplyLanguage();
    }

    partial void OnSelectedProviderChanged(ProviderDisplayItem? value)
    {
        if (value != null)
        {
            ApiKeyHint = _lang.GetProviderApiKeyHint(value.Type);
            StatusMessage = _lang.StatusSelected(value.DisplayName);
        }
    }

    /// <summary>
    /// 刷新所有绑定到 Lang 的属性通知
    /// </summary>
    private void ApplyLanguage()
    {
        // 通知所有绑定到 Lang 属性的 UI 更新
        OnPropertyChanged(string.Empty);

        // 更新非绑定的状态
        QueryButtonText = _lang.QueryButton;
        StatusMessage = _lang.StatusReady;
        ApiKeyHint = _lang.StatusSelectProvider;
    }

    /// <summary>
    /// 刷新提供商下拉列表
    /// </summary>
    private void RefreshProviders()
    {
        var previousType = SelectedProvider?.Type;
        Providers.Clear();

        foreach (ProviderType pt in Enum.GetValues(typeof(ProviderType)))
        {
            Providers.Add(new ProviderDisplayItem(pt,
                _lang.GetProviderDisplayName(pt),
                _lang.GetProviderApiKeyHint(pt)));
        }

        // 恢复之前的选择
        if (previousType.HasValue)
        {
            SelectedProvider = Providers.FirstOrDefault(p => p.Type == previousType.Value);
        }
    }

    // === 命令 ===

    [RelayCommand]
    private async Task QueryAsync()
    {
        if (SelectedProvider == null)
        {
            StatusMessage = "❌ " + _lang.StatusSelectProvider;
            return;
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            StatusMessage = "❌ " + _lang.StatusEnterApiKey;
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            QueryButtonText = _lang.QueryingButton;
            Models.Clear();
            HasResults = false;
            StatusMessage = _lang.StatusQuerying(SelectedProvider.DisplayName);

            var service = _factory.Create(SelectedProvider.Type);
            var models = await service.GetModelsAsync(ApiKey, _cts.Token);

            for (int i = 0; i < models.Count; i++)
            {
                Models.Add(new ModelItemViewModel(models[i], i + 1));
            }

            ResultCount = models.Count;
            HasResults = models.Count > 0;
            StatusMessage = models.Count > 0
                ? _lang.StatusSuccess(models.Count)
                : _lang.StatusNoModels;
        }
        catch (HttpRequestException ex)
        {
            var errorMsg = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => _lang.ErrUnauthorized,
                System.Net.HttpStatusCode.Forbidden => _lang.ErrForbidden,
                System.Net.HttpStatusCode.TooManyRequests => _lang.ErrTooManyRequests,
                _ => _lang.ErrNetwork(ex.Message)
            };
            StatusMessage = $"❌ {errorMsg}";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = _lang.StatusCancelled;
        }
        catch (Exception ex)
        {
            StatusMessage = _lang.ErrQueryFailed(ex.Message);
        }
        finally
        {
            IsLoading = false;
            QueryButtonText = _lang.QueryButton;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (!HasResults || Models.Count == 0) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = _lang.CsvFilter,
            FileName = _lang.CsvFileName(SelectedProvider?.DisplayName ?? "models"),
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var modelList = Models.Select(m => m.ToModelInfo()).ToList();
                await _exportService.ExportToCsvAsync(modelList, dialog.FileName,
                    _lang.HeaderIndex, _lang.HeaderModelId, _lang.HeaderDisplayName,
                    _lang.HeaderProvider, _lang.HeaderCreatedDate, _lang.HeaderOwner);
                StatusMessage = _lang.CsvExported(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = _lang.ExportFailed(ex.Message);
            }
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (!HasResults || Models.Count == 0) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = _lang.ExcelFilter,
            FileName = _lang.ExcelFileName(SelectedProvider?.DisplayName ?? "models"),
            DefaultExt = ".xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var modelList = Models.Select(m => m.ToModelInfo()).ToList();
                await _exportService.ExportToExcelAsync(modelList, dialog.FileName,
                    _lang.HeaderIndex, _lang.HeaderModelId, _lang.HeaderDisplayName,
                    _lang.HeaderProvider, _lang.HeaderCreatedDate, _lang.HeaderOwner);
                StatusMessage = _lang.ExcelExported(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = _lang.ExportFailed(ex.Message);
            }
        }
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (!HasResults || Models.Count == 0) return;

        try
        {
            var modelList = Models.Select(m => m.ToModelInfo()).ToList();
            var text = _exportService.GenerateClipboardText(modelList,
                _lang.HeaderIndex, _lang.HeaderModelId, _lang.HeaderDisplayName,
                _lang.HeaderProvider, _lang.HeaderCreatedDate, _lang.HeaderOwner);
            Clipboard.SetText(text);
            StatusMessage = _lang.Copied(modelList.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = _lang.CopyFailed(ex.Message);
        }
    }
}
