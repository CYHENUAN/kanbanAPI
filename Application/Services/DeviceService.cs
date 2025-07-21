using Application.Events;
using Application.Interfaces;
using Contracts;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Data.Interface;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly MESDBContext _db;
        private readonly IConfiguration _configuration;

        public DeviceService(IDeviceRepository repo, IMediator mediator, MESDBContext db, IConfiguration configuration)
        {
            _repo = repo;
            _mediator = mediator;
            _db = db;
            _configuration = configuration;
        }
        public async Task<List<DeviceDto>> GetAllAsync()
        {
            /*var list = await _repo.GetAllAsync();
            return list.Select(d => new DeviceDto { Id = d.Id, Name = d.Name, Status = d.Status.ToString() }).ToList();*/
            //获取当天的开始和结束时间

            var equimentStatusType = _configuration.GetSection("EquimentType").Get<EquimentTypeConfig>();
            if(equimentStatusType == null)
            {
                throw new Exception("EquimentType configuration is missing or invalid.");
            }

            var startOfDay = DateTime.Today;
            var endOfDay = DateTime.Today.AddDays(1);

            var latestIssuePerStation = _db.Events
                .Where(e => e.IssueDateTime >= startOfDay && e.IssueDateTime < endOfDay)
                .GroupBy(e => e.StationId)
                .Select(g => new {
                    StationId = g.Key,
                    LatestTime = g.Max(x => x.IssueDateTime)
                });

            var equimentStatus = await _db.Events
                .Join(latestIssuePerStation,
                      e => new { e.StationId, e.IssueDateTime },
                      g => new { g.StationId, IssueDateTime = g.LatestTime },
                      (e, g) => e)
                .Join(_db.Stations,
                      e => e.StationId,
                      s => s.Id,
                      (e, s) => new { e, s })
                .Select(x => new DeviceDto
                {
                    DeviceName = x.s.StationNumber ?? "Unknown",
                    Status = x.e.EventCategoryId == equimentStatusType.Running ? "running" :
                             x.e.EventCategoryId == equimentStatusType.ShutDown ? "fault" :
                             x.e.EventCategoryId == equimentStatusType.Standby ? "standby" : "unknown"
                })
                .ToListAsync();

            return equimentStatus;
        }
        public async Task<DeviceDto> UpdateStatusAsync(string name, string status)
        {
            //var device = await _repo.GetByIdAsync(id);
            //if (device == null) throw new Exception("Device not found");

            //device.UpdateStatus(Enum.Parse<DeviceStatus>(status));
            //await _repo.SaveAsync(device);

            var device = await _db.Stations.FirstOrDefaultAsync(s => s.StationNumber == name) ?? throw new Exception($"Device with name {name} not found.");
            
            var dto = new DeviceDto { DeviceName = name, Status = status };
            //发布设备状态变更通知
            await _mediator.Publish(new DeviceStatusChangedNotification { Device = dto });
            return dto;
        }
    }
}
