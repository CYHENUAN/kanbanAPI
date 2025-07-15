using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class OutPutQuantity
    {
        /// <summary>
        /// 月上线数量
        /// </summary>
        public int MonthOnlineQuantity { get; set; }
        /// <summary>
        /// 月下线数量
        /// </summary>
        public int MonthOutPutQuantity { get; set; }
        /// <summary>
        /// 当天上线数量
        /// </summary>
        public int DayOnlineQuantity { get; set; }
        /// <summary>
        /// 当天产出数量
        /// </summary>
        public int DayOutPutQuantity { get; set; }
    }
}
