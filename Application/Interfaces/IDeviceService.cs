using Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDeviceService
    {
        Task<DeviceDto> UpdateStatusAsync(long id, string status);
        Task<List<DeviceDto>> GetAllAsync();
    }

    public class DeviceStatusChangedNotification : INotification
    {
        public DeviceDto Device { get; set; } = null!;
    }
}
