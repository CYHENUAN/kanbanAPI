using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class EquimentTypeConfig
    {
        /// <summary>
        /// 停机
        /// </summary>
        public int ShutDown { get; set; }
        /// <summary>
        /// 维护
        /// </summary>
        public int Maintenance { get; set; }
        /// <summary>
        /// 运行
        /// </summary>
        public int Running { get; set; }
        /// <summary>
        /// 空闲 - 待机
        /// </summary>
        public int Standby { get; set; }
    }
}
