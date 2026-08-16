using Nadixa.Core.Attributes;
using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpreadCheetah;
using SpreadCheetah.Styling;
using System.Drawing;
using Nadixa.Application.Interfaces.Excel;
using Nadixa.Application.DTOS;
using ClosedXML.Excel;


namespace Nadixa.Infrastructure.Services.Excel
{
    public class ExcelHelperService : IExcelHelperService
    {
        private static readonly char[] InvalidSheetChars = { '\\', '/', '?', '*', '[', ']', ':' };

        public List<ExcelColumnInfo> ResolveColumns<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columns = properties
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<ExcelColumnAttribute>()
                })
                .Where(x => x.Attribute != null)
                .Select(x => new ExcelColumnInfo
                {
                    Header = x.Attribute!.Header,
                    Order = x.Attribute.Order,
                    Property = x.Property
                })
                .OrderBy(c => c.Order)
                .ToList();
            if (columns.Count == 0)
                throw new InvalidOperationException(
                    $"Type '{typeof(T).Name}' has no properties decorated with [ExcelColumn].");

            return columns;
        }

        public async Task<Spreadsheet> OpenSheetAsync(Stream stream, string sheetName, CancellationToken ct = default)
        {
            var validSheetName = CheckSheetName(sheetName);

            var spreadsheet = await SpreadCheetah.Spreadsheet.CreateNewAsync(stream, cancellationToken: ct);
            await spreadsheet.StartWorksheetAsync(validSheetName,null, ct);

            return spreadsheet;
        }

        public async Task WriteHeaderAsync(Spreadsheet spreadsheet, List<ExcelColumnInfo> columns, CancellationToken ct = default)
        {
            var headerStyle = new Style();
            headerStyle.Font.Bold = true;
            headerStyle.Font.Color = Color.White;
            headerStyle.Fill.Color = Color.FromArgb(0x6c, 0x7a, 0xe0);

            var styleId = spreadsheet.AddStyle(headerStyle);

            var headerCells = columns
                .Select(c => new StyledCell(c.Header, styleId))
                .ToArray();

            await spreadsheet.AddRowAsync(headerCells, ct);
        }
        public string CheckSheetName(string sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException("Sheet name cannot be empty.");

            var cleaned = new string(sheetName
                .Where(c => !InvalidSheetChars.Contains(c))
                .ToArray());

            if (cleaned.Length > 31)
                cleaned = cleaned[..31];

            return cleaned;
        }

    }
}
