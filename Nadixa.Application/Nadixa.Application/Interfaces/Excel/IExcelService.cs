using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces.Excel
{
    public interface IExcelService 
    {
        // بيصدّر أي IAsyncEnumerable<T> لملف إكسيل ويرجع stream جاهز للتنزيل
        Task<Stream> ExportAsync<T>(
            IAsyncEnumerable<T> data,
            string sheetName,
            CancellationToken ct = default);

        // Overload لو عندك List جاهزة مش async stream
        Task<Stream> ExportAsync<T>(
            IEnumerable<T> data,
            string sheetName,
            CancellationToken ct = default);

    }
}
