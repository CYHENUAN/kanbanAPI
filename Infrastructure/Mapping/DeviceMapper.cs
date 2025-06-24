using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mapping
{
    public static class DeviceMapper
    {
        public static Device ToDomain(StationStatusHi db)
        => new Device(db.Id, db.StationId.ToString())
        {
            Status = Enum.IsDefined(typeof(DeviceStatus), db.Status)
                ? (DeviceStatus)db.Status
                : DeviceStatus.Unknown
        };

        public static StationStatusHi ToDb(Device domain)
            => new StationStatusHi
            {
                Id = domain.Id,
                StationId = int.TryParse(domain.Name, out var stationId) ? stationId : 0
            };
    }
}
