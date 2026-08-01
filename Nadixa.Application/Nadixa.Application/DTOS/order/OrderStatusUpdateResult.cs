using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.order
{
    public class OrderStatusUpdateResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static OrderStatusUpdateResult Ok() => new() { Success = true };

        public static OrderStatusUpdateResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
