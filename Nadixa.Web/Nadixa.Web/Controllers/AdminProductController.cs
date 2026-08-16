using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces.Excel;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Services;
using Nadixa.Application.DTOS.Product;
using Nadixa.Application.Interfaces;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IProductImportExportService _importExportService;

        public AdminProductController(IProductImportExportService importExportService)
        {
            _importExportService = importExportService;
        }


        public async Task<IActionResult> ExportToExcel(CancellationToken ct = default)
        {
            var stream = await _importExportService.ExportToExcelAsync(ct);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Products_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index", "AdminDashboard");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                TempData["Error"] = "Only .xlsx files are allowed.";
                return RedirectToAction("Index", "AdminDashboard");
            }
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var result = await _importExportService.ImportFromExcelAsync(stream);

            TempData["Success"] = $"Import completed: {result.Created} created, {result.Updated} updated, {result.Failed} failed.";

            if (result.NewCategories > 0 || result.NewSubCategories > 0)
            {
                TempData["Warning"] = $"{result.NewCategories} new category(s) and {result.NewSubCategories} new subcategory(s) were created without images. Please add images from the Categories page.";
            }
            if (result.Errors.Any())
                TempData["Error"] = string.Join(" | ", result.Errors);

            return RedirectToAction("Index", "AdminDashboard");

        }
    }
}