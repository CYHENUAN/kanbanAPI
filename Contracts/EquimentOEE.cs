using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class EquimentOEE
    {
        /// <summary>
        /// 工站编号
        /// </summary>
        public string StationNumber { get; set; } = string.Empty;
        /// <summary>
        /// 工站名称
        /// </summary>
        public string StationDescription { get; set; } = string.Empty;
        /// <summary>
        /// 设备可用率（OEE）
        /// </summary>
        public double Availability { get; set; }
    }
}
