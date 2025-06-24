using Domain.Entities;
using Infrastructure.Data.Interface;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class DeviceRepository: IDeviceRepository
    {
        private readonly MESDBContext _db;

        public DeviceRepository(MESDBContext db)
        {
            _db = db;
        }

        public async Task<Device?> GetByIdAsync(long id)
        {
            var dbDevice = await _db.StationStatusHis.FindAsync(id);
            return dbDevice == null ? null : DeviceMapper.ToDomain(dbDevice);
        }

        public async Task<List<Device>> GetAllAsync()
        {
            var list = await _db.StationStatusHis.ToListAsync();
            return list.Select(DeviceMapper.ToDomain).ToList();
        }

        public async Task SaveAsync(Device device)
        {
            var dbDevice = DeviceMapper.ToDb(device);
            _db.Update(dbDevice);
            await _db.SaveChangesAsync();
        }
    }
}
