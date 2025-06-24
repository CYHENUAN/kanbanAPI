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
        public VisualKanbanAPIController(IDeviceService service)
        {
            _service = service;
        }
        /// <summary>
        /// 更新设备状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<DeviceDto>> UpdateDeviceStatusAsync(UpdateStatusRequest request)
        {
            var result = await _service.UpdateStatusAsync(request.DeviceId, request.Status);
            return ApiResult<DeviceDto>.Ok(result);
        }
        /// <summary>
        /// 获取所有设备状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<DeviceDto>>> GetAllDeviceStatusAsync()
        {
            var result = await _service.GetAllAsync();
            if(result.Count == 0)
                return ApiResult<List<DeviceDto>>.Fail("No devices found");
            return ApiResult<List<DeviceDto>>.Ok(result);
        }
    }
}
