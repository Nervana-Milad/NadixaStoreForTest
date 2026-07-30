using Nadixa.Application.DTOS;
using Nadixa.Core.DTOS;
using SpreadCheetah;


namespace Nadixa.Application.Interfaces.Excel
{
    public interface IExcelHelperService
    {
        // بترجع الأعمدة اللي عليها [ExcelColumn] Attribute مرتبة بالـ Order
        List<ExcelColumnInfo> ResolveColumns<T>();

        // بتفتح الـ Spreadsheet وتبدأ الـ worksheet بالاسم اللي هيتعمله check الأول
        Task<Spreadsheet> OpenSheetAsync(Stream stream, string sheetName, CancellationToken ct = default);

        // بتكتب صف الهيدر (Bold + خلفية لون) باستخدام الأعمدة اللي جاية من ResolveColumns
        Task WriteHeaderAsync(Spreadsheet spreadsheet, List<ExcelColumnInfo> columns, CancellationToken ct = default);

        // بتتأكد إن اسم الشيت valid (Excel بيرفض أكتر من 31 حرف وبعض الرموز) وترجعه نضيف
        string CheckSheetName(string sheetName);
    }
}
