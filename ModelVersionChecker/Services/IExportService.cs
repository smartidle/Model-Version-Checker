using ModelVersionChecker.Models;

namespace ModelVersionChecker.Services;

/// <summary>
/// 导出服务接口
/// </summary>
public interface IExportService
{
    /// <summary>导出为 CSV 文件</summary>
    Task ExportToCsvAsync(List<ModelInfo> models, string filePath,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner);

    /// <summary>导出为 Excel 文件</summary>
    Task ExportToExcelAsync(List<ModelInfo> models, string filePath,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner);

    /// <summary>生成剪贴板文本</summary>
    string GenerateClipboardText(List<ModelInfo> models,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner);
}
