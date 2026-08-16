using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Product
{
    public class ProductImportResultDto
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public int NewCategories { get; set; }
        public int NewSubCategories { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
