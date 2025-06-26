using Application.Interfaces;
using Contracts;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductionService : IProductionService
    {
        private readonly MESDBContext _db;
        private readonly IConfiguration _configuration;
        public ProductionService(MESDBContext db, IConfiguration configuration)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "Database context cannot be null.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
        }
        public async Task<List<ProductInformation>> GetAllProductNumbersInformationAsync()
        {
            // 从配置文件中获取起始站点编号，如果未配置则默认为"OP010"
            string? station = _configuration["StartStation"];
            if(station == null)
            {
                throw new ArgumentNullException(nameof(station), "outputStation cannot be null.");
            }

            // 查询当前时间往前一天24小时内的所有产品信息
            var productInfo = await _db.PartSerialNumbers
                .Join(_db.PartProcessRecords,
                    A => A.Id,
                    B => B.PartSerialNumberId,
                    (A, B) => new { A, B })
                .Join(_db.Stations,
                AB => AB.B.StationId,
                C => C.Id,
                (AB, C) => new { AB, C })
                .Where(x => x.AB.B.BookDateTime >= DateTime.Now.AddDays(-1) && x.C.StationNumber == station)
                .Select(x => new ProductInformation
                {
                    SerialNumber = x.AB.A.SerialNumber,
                    MaterialNumber = x.AB.A.Material != null ? x.AB.A.Material.MaterialNumber ?? string.Empty : string.Empty,        
                    BookDateTime = x.AB.B.BookDateTime
                })
                .OrderBy(x => x.BookDateTime)
                .ToListAsync();      
            
            return productInfo;
        }

        public async Task<OutPutQuantity> GetOutputQuantityAsync()
        {
            //从配置获取产出工位
            var outputStation = _configuration["OutPutStation"];
            if (outputStation == null)
            {
                throw new ArgumentNullException(nameof(outputStation), "outputStation cannot be null.");
            }

            //获取当天的时间  00:00:00 - 24:00:00
            DateTime startOfDay = DateTime.Today;
            DateTime endOfDay = DateTime.Today.AddDays(1);

            //获取当月时间
            DateTime now = DateTime.Now;
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            //查询当月产量数据
            var MonthOutput = await _db.PartSerialNumbers
               .Join(_db.PartProcessRecords,
                   A => A.Id,
                   B => B.PartSerialNumberId,
                   (A, B) => new { A, B })
               .Join(_db.Stations,
               AB => AB.B.StationId,
               C => C.Id,
               (AB, C) => new { AB, C })
               .Where(x => x.AB.B.BookDateTime >= startOfMonth && x.AB.B.BookDateTime < startOfNextMonth && x.C.StationNumber == outputStation)
               .Select(x => new
               {
                   x.AB.A.SerialNumber,
                   x.AB.B.BookDateTime,
               })
               .DistinctBy(x => x.SerialNumber)
               .ToListAsync();

            var dayOutput = MonthOutput.Where(x => x.BookDateTime >= startOfDay && x.BookDateTime < endOfDay).ToList();

            OutPutQuantity outPutQuantity = new OutPutQuantity
            {
                MonthQuantity = MonthOutput.Count(),
                DayQuantity = dayOutput.Count()
            };

            return outPutQuantity;
        }

        public Task<object> UpdateProductQuantityAsync(string serialnumber)
        {






            throw new NotImplementedException();
        }
    }
}
