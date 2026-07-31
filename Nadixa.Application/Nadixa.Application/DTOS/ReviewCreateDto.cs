using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
