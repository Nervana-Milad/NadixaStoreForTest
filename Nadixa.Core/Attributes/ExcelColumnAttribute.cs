using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumnAttribute : Attribute
    {
        public string Header { get; }
        public int Order { get; }

        public ExcelColumnAttribute(string header, int order = 0)
        {
            Header = header;
            Order = order;
        }
    }
}
