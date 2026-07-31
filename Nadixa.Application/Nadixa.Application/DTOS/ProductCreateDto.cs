using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public int ProductSubCategoryId { get; set; }
        public FileUploadRequest? MainImage { get; set; }
        public List<FileUploadRequest>? GalleryImages { get; set; }
    }
}
