using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Device
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public DeviceStatus Status { get; set; }

        public Device(long id, string name)
        {
            Id = id;
            Name = name;
            Status = DeviceStatus.Unknown;
        }

        public void UpdateStatus(DeviceStatus newStatus)
        {
            if (Status != newStatus)
            {
                Status = newStatus;
            }
        }
    }
}
