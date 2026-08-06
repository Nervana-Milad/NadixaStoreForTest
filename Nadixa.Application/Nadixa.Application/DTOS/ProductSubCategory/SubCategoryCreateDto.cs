using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.ProductSubCategory
{
    public class SubCategoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCategoryId { get; set; }
        public FileUploadRequest? Image { get; set; }
    }
}
