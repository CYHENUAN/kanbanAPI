using Application.Interfaces;
using Contracts;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VisualKanbanWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VisualKanbanAPIController : ControllerBase
    {
        private readonly IDeviceService _service;
        private readonly IProductionService _productionService;
        private readonly ILogger<VisualKanbanAPIController> _logger;

        public VisualKanbanAPIController(IDeviceService service, IProductionService production, ILogger<VisualKanbanAPIController> logger)
        {
            _service = service;
            _productionService = production;
            _logger = logger;
        }

        /// <summary>
        /// 统一异常处理方法
        /// </summary>
        private async Task<ApiResult<T>> HandleRequestAsync<T>(Func<Task<T>> func)
        {
            try
            {
                var result = await func();
                return ApiResult<T>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "请求处理发生异常");
                return ApiResult<T>.Fail("服务器内部错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 更新设备状态
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<DeviceDto>> UpdateDeviceStatusAsync(UpdateStatusRequest request)
        {
            return await HandleRequestAsync(async () =>
            {
                return await _service.UpdateStatusAsync(request.DeviceId, request.Status);
            });
        }

        /// <summary>
        /// 获取所有设备状态
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<List<DeviceDto>>> GetAllDeviceStatusAsync()
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await _service.GetAllAsync();
                if (result.Count == 0)
                    throw new Exception("No devices found");
                return result;
            });
        }

        /// <summary>
        /// 获取当前时间到前一天24小时内的所有产品信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProductInformation>>> GetProductNumbersAsync()
        {
            return await HandleRequestAsync(async () =>
            {
                return await _productionService.GetAllProductNumbersInformationAsync();
            });
        }
        /// <summary>
        /// 获取当月以及当天的产品产量 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<OutPutQuantity>> GetProductOutputQuantityAsync()
        {
            return await HandleRequestAsync(async () =>
            {
                return await _productionService.GetOutputQuantityAsync();
            });
        }

        [HttpPost]
        public async Task<ApiResult<object>> UpDateProductInfromationAsync(string serialnumber,string station = "")
        {
            if (string.IsNullOrWhiteSpace(serialnumber))
            {
                return ApiResult<object>.Fail("serialnumber cannot be null.");
            }
            return await HandleRequestAsync(async () =>
            {
                return await _productionService.UpdateProductQuantityAsync(serialnumber, station);                 
            });
        }
    }
}
