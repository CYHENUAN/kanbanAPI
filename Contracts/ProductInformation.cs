using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ProductInformation
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string? MaterialNumber { get; set; }
        public DateTime? BookDateTime { get; set; }
    }
}
