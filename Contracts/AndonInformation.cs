using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class AndonInformation
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 工序号
        /// </summary>
        public string ProcessNumber { get; set; } = string.Empty;
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; } = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public string AndonType { get; set; } = string.Empty;
        /// <summary>
        /// 处理进度
        /// </summary>
        public string ProcessingProgress {  get; set; } = string.Empty;
    }
}
