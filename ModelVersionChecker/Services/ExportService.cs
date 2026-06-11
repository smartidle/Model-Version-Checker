using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ClosedXML.Excel;
using ModelVersionChecker.Models;

namespace ModelVersionChecker.Services;

/// <summary>
/// 导出服务实现，支持 CSV 和 Excel 格式
/// </summary>
public class ExportService : IExportService
{
    public async Task ExportToCsvAsync(List<ModelInfo> models, string filePath,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // 写入表头
        csv.WriteField(hIndex);
        csv.WriteField(hModelId);
        csv.WriteField(hDisplayName);
        csv.WriteField(hProvider);
        csv.WriteField(hCreatedDate);
        csv.WriteField(hOwner);
        await csv.NextRecordAsync();

        // 写入数据
        for (int i = 0; i < models.Count; i++)
        {
            var model = models[i];
            csv.WriteField(i + 1);
            csv.WriteField(model.Id);
            csv.WriteField(model.DisplayName);
            csv.WriteField(model.Provider);
            csv.WriteField(model.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss"));
            csv.WriteField(model.OwnedBy ?? "");
            await csv.NextRecordAsync();
        }
    }

    public async Task ExportToExcelAsync(List<ModelInfo> models, string filePath,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(hProvider);

            // 表头
            var headers = new[] { hIndex, hModelId, hDisplayName, hProvider, hCreatedDate, hOwner };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // 数据行
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                int row = i + 2;

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = model.Id;
                worksheet.Cell(row, 3).Value = model.DisplayName;
                worksheet.Cell(row, 4).Value = model.Provider;
                worksheet.Cell(row, 5).Value = model.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                worksheet.Cell(row, 6).Value = model.OwnedBy ?? "";

                // 交替行颜色
                if (i % 2 == 1)
                {
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
                    }
                }
            }

            // 自适应列宽
            worksheet.Columns().AdjustToContents();

            // 冻结首行
            worksheet.SheetView.FreezeRows(1);

            // 添加边框
            var range = worksheet.Range(1, 1, models.Count + 1, headers.Length);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            workbook.SaveAs(filePath);
        });
    }

    public string GenerateClipboardText(List<ModelInfo> models,
        string hIndex, string hModelId, string hDisplayName, string hProvider, string hCreatedDate, string hOwner)
    {
        var sb = new StringBuilder();

        // 表头
        sb.AppendLine($"{hIndex}\t{hModelId}\t{hDisplayName}\t{hProvider}\t{hCreatedDate}\t{hOwner}");

        // 数据
        for (int i = 0; i < models.Count; i++)
        {
            var model = models[i];
            sb.AppendLine($"{i + 1}\t{model.Id}\t{model.DisplayName}\t{model.Provider}\t{model.CreatedDate?.ToString("yyyy-MM-dd") ?? ""}\t{model.OwnedBy ?? ""}");
        }

        return sb.ToString();
    }
}
