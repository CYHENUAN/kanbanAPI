using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Interface
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(long id);
        Task<List<Device>> GetAllAsync();
        Task SaveAsync(Device device);
    }
}
