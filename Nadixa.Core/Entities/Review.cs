using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Review : BaseEntity
    {
        public string UserName { get; set; }

        public string? UserImage { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        // Foreign Key
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

