using Application.Events;
using Application.Interfaces;
using Contracts;
using Infrastructure.Data;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        private readonly IMediator _mediator;
        private readonly RedisCacheService _redisCache;
        public ProductionService(MESDBContext db, IConfiguration configuration, IMediator mediator, RedisCacheService redisCache)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "Database context cannot be null.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator), "Mediator cannot be null.");
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache), "Redis cache service cannot be null.");
        }
        public async Task<List<ProductInformation>> GetAllProductNumbersInformationAsync()
        {
            // 从配置文件中获取起始站点编号，如果未配置则默认为"OP010"
            string? station = _configuration["StartStation"];
            if(station == null)
            {
                throw new ArgumentNullException(nameof(station), "outputStation cannot be null.");
            }
            var outputStation = _configuration["OutPutStation"];
            if (outputStation == null)
            {
                throw new ArgumentNullException(nameof(outputStation), "outputStation cannot be null.");
            }
           
            // 查询当前时间往前一天24小时内的所有产品信息
            var startStaion = await _db.PartSerialNumbers
                .Join(_db.PartProcessRecords,
                    A => A.Id,
                    B => B.PartSerialNumberId,
                    (A, B) => new { A, B })
                .Join(_db.Stations,
                AB => AB.B.StationId,
                C => C.Id,
                (AB, C) => new { AB, C })
                .Where(x => x.AB.B.BookDateTime >= DateTime.Now.AddDays(-1) && x.C.StationNumber == station )
                .Select(x => new ProductInformation
                {
                    SerialNumber = x.AB.A.SerialNumber,
                    MaterialNumber = x.AB.A.Material != null ? x.AB.A.Material.MaterialNumber ?? string.Empty : string.Empty,        
                    BookDateTime = x.AB.B.BookDateTime
                })
                .OrderBy(x => x.BookDateTime)
                .GroupBy(x => x.SerialNumber)
                .Select(x => x.First())
                .ToListAsync();  

            var outStation = await _db.PartSerialNumbers
                .Join(_db.PartProcessRecords,
                    A => A.Id,
                    B => B.PartSerialNumberId,
                    (A, B) => new { A, B })
                .Join(_db.Stations,
                AB => AB.B.StationId,
                C => C.Id,
                (AB, C) => new { AB, C })
                .Where(x => x.AB.B.BookDateTime >= DateTime.Now.AddDays(-1) && x.C.StationNumber == outputStation)
                .Select(x => new ProductInformation
                {
                    SerialNumber = x.AB.A.SerialNumber,
                    MaterialNumber = x.AB.A.Material != null ? x.AB.A.Material.MaterialNumber ?? string.Empty : string.Empty,
                    BookDateTime = x.AB.B.BookDateTime
                })
                .OrderBy(x => x.BookDateTime)
                .GroupBy(x => x.SerialNumber)
                .Select(x => x.First())
                .ToListAsync();

            var productInfo = startStaion.Where(x => !outStation.Any(g => g.SerialNumber == x.SerialNumber)).ToList();

            return productInfo;
        }

        public async Task<OutPutQuantity> GetOutputQuantityAsync()
        {
            //从配置获取上线工位
            var StartStation = _configuration["StartStation"];
            if (StartStation == null)
            {
                throw new ArgumentNullException(nameof(StartStation), "StartStation cannot be null.");
            }
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

            //查询当月上线数量
            var MonthOnline = await _db.PartSerialNumbers
               .Join(_db.PartProcessRecords,
                   A => A.Id,
                   B => B.PartSerialNumberId,
                   (A, B) => new { A, B })
               .Join(_db.Stations,
               AB => AB.B.StationId,
               C => C.Id,
               (AB, C) => new { AB, C })
               .Where(x => x.AB.B.BookDateTime >= startOfMonth && x.AB.B.BookDateTime < startOfNextMonth && x.C.StationNumber == StartStation)
               .Select(x => new
               {
                   x.AB.A.SerialNumber,
                   x.AB.B.BookDateTime,
               })
              .GroupBy(x => x.SerialNumber)
              .Select(g => g.Last())
              .ToListAsync();

            //查询当月产出数量
            var MonthOutPut = await _db.PartSerialNumbers
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
              .GroupBy(x => x.SerialNumber)
              .Select(g => g.LastOrDefault())
              .ToListAsync();

            var dayOnline = MonthOnline.Count(x => x.BookDateTime >= startOfDay && x.BookDateTime < endOfDay);
            var dayOutput = MonthOutPut.Count(x => x.BookDateTime >= startOfDay && x.BookDateTime < endOfDay);

            OutPutQuantity outPutQuantity = new OutPutQuantity
            {
                MonthOnlineQuantity = MonthOnline.Count,
                MonthOutPutQuantity = MonthOutPut.Count,
                DayOnlineQuantity = dayOnline,
                DayOutPutQuantity = dayOutput
            };

            //将产量信息发送到消息队列
            await _mediator.Publish(new ProductInfromationNotification { Type = "ReceiveProductQuantityInformation", OutPutQuantity = outPutQuantity });
            
            return outPutQuantity;
        }

        public async Task<string> UpdateProductQuantityAsync(ProductUpdateRequest product)
        {
            var outputStation = _configuration["OutPutStation"];
            if (outputStation == null)
            {
                throw new ArgumentNullException(nameof(outputStation), "outputStation cannot be null.");
            }

            var productInfo = await _db.PartSerialNumbers
                .Join(_db.Materials, A => A.MaterialId, B => B.Id, (A, B) => new { A, B })
                .Where(x => x.A.SerialNumber == product.SerialNumber)
                .Select(x => new ProductItem
                {
                    Serial = x.A.SerialNumber,
                    Material = x.B.MaterialNumber
                }).FirstOrDefaultAsync();

            if(product == null)
            {
                return "Product not found.";
            }

           //判断产品是否在产出站
            if(product.Station == outputStation)
            {              
                await _mediator.Publish(new ProductInfromationNotification { Type = "ReceiveB", SerialNumber = productInfo });
            }
            else
            {
                //触发产品信息变更通知
                await _mediator.Publish(new ProductInfromationNotification { Type = "ReceiveA", SerialNumber = productInfo });
            }
            return "Product Upload Sucess.";           
        }


        private async Task<List<StationInformation>> GetStationAsync(string lineName)
        {
            if (await _redisCache.ExistsAsync("stationList"))
            {
                var cachedStationList = await _redisCache.GetAsync<List<StationInformation>>("stationList");
                if (cachedStationList == null)
                {
                    throw new InvalidOperationException("Cached station list is null.");
                }
                return cachedStationList;
            }
            else
            {
                var stationList = await _db.Station_StationGroups
                    .Join(_db.Stations, A => A.StationId, B => B.Id, (A, B) => new { A, B })
                    .Join(_db.StationGroups, AB => AB.A.StationGroupId, C => C.Id, (AB, C) => new { AB, C })
                    .Join(_db.Cell_Stations, ABC => ABC.AB.A.StationId, D => D.StationId, (ABC, D) => new { ABC, D })
                    .Join(_db.Cells, ABCD => ABCD.D.CellId, E => E.Id, (ABCD, E) => new { ABCD, E })
                    .Where(x => x.ABCD.ABC.C.CellNumber == lineName)
                    .Select(x => new StationInformation
                    {
                        StationId = x.ABCD.AB.B.Id,
                        StationNumber = x.ABCD.AB.B.StationNumber,
                        StationgroupId = x.ABCD.ABC.C.Id
                    })
                    .ToListAsync();

                return stationList;
            }
        }
    }
}
