using Application.Events;
using Application.Interfaces;
using Contracts;
using Domain.Enums;
using Infrastructure.Data.Interface;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repo;
        private readonly IMediator _mediator;

        public DeviceService(IDeviceRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }
        public async Task<List<DeviceDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(d => new DeviceDto { Id = d.Id, Name = d.Name, Status = d.Status.ToString() }).ToList();
        }
        public async Task<DeviceDto> UpdateStatusAsync(long id, string status)
        {
            var device = await _repo.GetByIdAsync(id);
            if (device == null) throw new Exception("Device not found");

            device.UpdateStatus(Enum.Parse<DeviceStatus>(status));
            await _repo.SaveAsync(device);

            var dto = new DeviceDto { Id = device.Id, Name = device.Name, Status = device.Status.ToString() };
            await _mediator.Publish(new DeviceStatusChangedNotification { Device = dto });
            return dto;
        }
    }
}
