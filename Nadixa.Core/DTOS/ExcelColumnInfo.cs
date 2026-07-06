using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.DTOS
{
    public class ExcelColumnInfo
    {
        public string Header { get; set; } = default!;
        public int Order { get; set; }
        public PropertyInfo Property { get; set; } = default!;
    }
}
