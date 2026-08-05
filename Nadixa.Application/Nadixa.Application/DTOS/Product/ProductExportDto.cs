using Nadixa.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Product
{
    public class ProductExportDto
    {
        [ExcelColumn("Id", 1)]
        public int Id { get; set; }

        [ExcelColumn("Name", 2)]
        public string Name { get; set; } = default!;

        [ExcelColumn("Description", 3)]
        public string Description { get; set; } = default!;

        [ExcelColumn("Price", 4)]
        public decimal Price { get; set; }

        [ExcelColumn("OldPrice", 5)]
        public decimal OldPrice { get; set; }

        [ExcelColumn("StockQuantity", 6)]
        public int StockQuantity { get; set; }
        [ExcelColumn("Category", 7)]
        public string CategoryName { get; set; } = default!;

        [ExcelColumn("SubCategory", 8)]
        public string SubCategoryName { get; set; } = default!;
    }
}
