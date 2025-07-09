using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ProductUpdateRequest
    {
        public string? SerialNumber { get; set; }
        public string? Station { get; set; }
    }
}
