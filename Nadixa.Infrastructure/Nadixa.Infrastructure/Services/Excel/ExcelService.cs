using ClosedXML.Excel;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces.Excel;
using SpreadCheetah;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services.Excel
{
    public class ExcelService : IExcelService
    {
        private readonly IExcelHelperService _helper;

        public ExcelService(IExcelHelperService helper)
        {
            _helper = helper;
        }

        public async Task<Stream> ExportAsync<T>(
            IAsyncEnumerable<T> data,
            string sheetName,
            CancellationToken ct = default)
        {
            var stream = new MemoryStream();

            var columns = _helper.ResolveColumns<T>();
            using var spreadsheet = await _helper.OpenSheetAsync(stream, sheetName, ct);

            await _helper.WriteHeaderAsync(spreadsheet, columns, ct);

            await foreach (var item in data.WithCancellation(ct))
            {
                var rowCells = BuildRowCells(item, columns);
                await spreadsheet.AddRowAsync(rowCells, ct);
            }

            await spreadsheet.FinishAsync(ct);
            stream.Position = 0;
            return stream;
        }

        public async Task<Stream> ExportAsync<T>(
            IEnumerable<T> data,
            string sheetName,
            CancellationToken ct = default)
        {
            return await ExportAsync(ToAsyncEnumerable(data), sheetName, ct);
        }

        private static DataCell[] BuildRowCells<T>(T item, List<ExcelColumnInfo> columns)
            {
                var cells = new DataCell[columns.Count];

                for (int i = 0; i < columns.Count; i++)
                {
                    var value = columns[i].Property.GetValue(item);
                    cells[i] = ToDataCell(value);
                }

                return cells;
            }

        private static DataCell ToDataCell(object? value)
        {
            return value switch
            {
                null => new DataCell(string.Empty),
                int i => new DataCell(i),
                decimal d => new DataCell(d),
                double db => new DataCell(db),
                DateTime dt => new DataCell(dt),
                bool b => new DataCell(b),
                _ => new DataCell(value.ToString() ?? string.Empty)
            };
        }

        private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
