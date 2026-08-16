using Nadixa.Application.DTOS.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IProductImportExportService
    {
        Task<Stream> ExportToExcelAsync(CancellationToken ct = default);
        Task<ProductImportResultDto> ImportFromExcelAsync(Stream fileStream, CancellationToken ct = default);

    }
}
