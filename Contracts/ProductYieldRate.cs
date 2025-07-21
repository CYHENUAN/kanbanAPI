using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ProductYieldRate
    {
        public string StationNumber { get; set; } = string.Empty;
        public int ToalQuantity { get; set; }

        public int PassQuantity
        {
            get
            {
                if (ToalQuantity == 0) return 0;
                return ToalQuantity - FailQuantity;
            }
        }

        public int FailQuantity { get; set; }

        public double YieldRate
        {
            get
            {
                if (ToalQuantity == 0) return 0;
                return Math.Round((double)PassQuantity / ToalQuantity * 100, 2);
            }
        }
    }
}
