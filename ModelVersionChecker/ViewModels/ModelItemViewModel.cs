using CommunityToolkit.Mvvm.ComponentModel;
using ModelVersionChecker.Models;

namespace ModelVersionChecker.ViewModels;

/// <summary>
/// DataGrid 中单个模型行项的 ViewModel
/// </summary>
public partial class ModelItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _modelId = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _provider = string.Empty;

    [ObservableProperty]
    private string _createdDate = string.Empty;

    [ObservableProperty]
    private string _ownedBy = string.Empty;

    public ModelItemViewModel(ModelInfo model, int index)
    {
        _index = index;
        _modelId = model.Id;
        _displayName = model.DisplayName;
        _provider = model.Provider;
        _createdDate = model.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
        _ownedBy = model.OwnedBy ?? "-";
    }

    /// <summary>
    /// 转换回 ModelInfo（用于导出）
    /// </summary>
    public ModelInfo ToModelInfo() => new()
    {
        Id = ModelId,
        DisplayName = DisplayName,
        Provider = Provider,
        CreatedDate = DateTime.TryParse(CreatedDate, out var date) ? date : null,
        OwnedBy = OwnedBy
    };
}
