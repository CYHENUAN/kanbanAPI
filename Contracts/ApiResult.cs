using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResult<T> Ok(T data, string? message = "操作成功")
            => new() { Success = true, Data = data, Message = message };

        public static ApiResult<T> Fail(string message)
            => new() { Success = false, Message = message };
    }
}
