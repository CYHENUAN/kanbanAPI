using Application.Events;
using Application.Interfaces;
using Contracts;
using Infrastructure.Data;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IDataQueryService _dataQueryService;
        public ProductionService(MESDBContext db, IConfiguration configuration, IMediator mediator, RedisCacheService redisCache, IDataQueryService dataQuery)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "Database context cannot be null.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator), "Mediator cannot be null.");
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache), "Redis cache service cannot be null.");
            _dataQueryService = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery), "Data query service cannot be null.");
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
              .Select(g => g.OrderBy(x => x.BookDateTime).Last())
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
              .Select(g => g.OrderBy(x => x.BookDateTime).Last())
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

        public async Task<List<ProductYieldRate>> GetProductYieldRateByStationAsync(DateTime? startOfDay, DateTime? endOfDay)
        {
            string lineName = _configuration["LineName"] ?? throw new ArgumentNullException(nameof(_configuration), "LineName cannot be null.");
            var stations = await GetStationAsync(lineName);

            if (stations == null || stations.Count == 0)
            {
                throw new ArgumentNullException(nameof(stations), "Stations cannot be null or empty.");
            }

            // 如果时间没有选定,默认获取当天的时间  00:00:00 - 24:00:00
            startOfDay ??= DateTime.Today;
            endOfDay ??= DateTime.Today.AddDays(1);

            // 查询当天每个站点的总数，合格数，不合格数
            var yieldRate = await _db.Stations
                .Join(_db.PartProcessRecords, A => A.Id, B => B.StationId, (A, B) => new { A, B })
                .Where(x => x.B.BookDateTime >= startOfDay.Value && x.B.BookDateTime < endOfDay.Value && stations.Contains(x.A.Id))
                .GroupBy(x => x.A.StationNumber)
                .Select(g => new ProductYieldRate
                {
                    StationNumber = g.Key,
                    ToalQuantity = g.Select(x => x.B.PartSerialNumberId).Distinct().Count(),
                    FailQuantity = g.Where(x => x.B.ProcessState == 1 || x.B.ProcessState == 2).Select(x => x.B.PartSerialNumberId).Distinct().Count()
                }).ToListAsync();

            return yieldRate;
        }

        public async Task<List<EquimentOEE>> GetEquimentOeeAsync(double workingHours, double planStopTime)
        {
            //获取线体名称
            string lineName = _configuration["LineName"] ?? throw new ArgumentNullException(nameof(_configuration), "LineName cannot be null.");
            //获取工站列表
            var stations = await GetStationAsync(lineName);

            if (stations == null || stations.Count == 0)
            {
                throw new ArgumentNullException(nameof(stations), "Stations cannot be null or empty.");
            }
            //获取工艺ID
            var processId = await _db.WorkOrders
                .Join(_db.WorkOrder_Cells, a => a.Id, b => b.WorkOrderId, (a, b) => new { a, b })
                .Join(_db.Cells, ab => ab.b.CellId, c => c.Id, (ab, c) => new { ab, c })
                .Where(x => x.c.CellNumber == lineName)
                .Select(x => x.ab.a.ProcessId)
                .FirstOrDefaultAsync();

            //获取工艺节拍
            var processCycle = await _db.ProcessSteps
                .Join(_db.Station_StationGroups, a => a.StationGroupId, b => b.StationGroupId, (a, b) => new {a,b})
                .Join(_db.Stations, ab => ab.b.StationId, c => c.Id, (ab, c) => new {ab, c})
                .Where(x => x.ab.a.ProcessId == processId && x.ab.a.State == 1 && stations.Contains(x.c.Id))
                .Select(x => new
                {
                    x.c.Id,
                    x.c.StationNumber,
                    x.c.Name,
                    CycleTime = x.ab.a.TimePerUnitInSeconds
                })
                .ToListAsync();

            //获取工站生产数据
            var startOfDay = DateTime.Today;
            var endOfDay = DateTime.Today.AddDays(1);

            var productionData = await _db.PartProcessRecords
                .Join(_db.Stations, A => A.StationId, B => B.Id, (A, B) => new { A, B })
                .Where(x => x.A.BookDateTime >= startOfDay && x.A.BookDateTime < endOfDay && stations.Contains(x.B.Id))
                .GroupBy(x => x.B.StationNumber)
                .Select(g => new
                {
                    StationNumber = g.Key,
                    TotalQuantity = g.Select(x => x.A.PartSerialNumberId).Distinct().Count(),                   
                }).ToListAsync();

            //获取工位停机损失时间
            var equimentType = _configuration.GetSection("EquimentType").Get<EquimentTypeConfig>();
            if(equimentType == null)
            {
                throw new ArgumentNullException(nameof(equimentType), "EquimentType configuration is missing or invalid.");
            }
            var downtimeLoss = await _db.Events
                .Where(x => x.IssueDateTime >= startOfDay &&
                x.IssueDateTime < endOfDay &&
                x.EventCategoryId == equimentType.ShutDown &&
                x.StationId.HasValue &&
                stations.Contains(x.StationId.Value))
                .GroupBy(x => x.StationId)
                .Select(g => new
                {
                    StationNumberId = g.Key,
                    Downtime = g.Sum(x => (x.IssueDateTime.HasValue && x.ActionFinishedDateTime.HasValue ) ? EF.Functions.DateDiffSecond(x.IssueDateTime.Value, x.ActionFinishedDateTime.Value) : 0)
                }).ToListAsync();

            //获取工站合格率
            var yieldRate = await GetProductYieldRateByStationAsync(startOfDay, endOfDay);

            List<EquimentOEE> equiments = new List<EquimentOEE>();
            //负荷时间 = 工作时间 - 停机时间
            var loadTime = workingHours * 60 - planStopTime;
            //计算OEE
            processCycle.ForEach(item =>
            {
                var downtimeLossTime = downtimeLoss.FirstOrDefault(x => x.StationNumberId == item.Id)?.Downtime ?? 0;
                var totalQuantity = productionData.FirstOrDefault(x => x.StationNumber == item.StationNumber)?.TotalQuantity ?? 0;
                var yieldRateItem = yieldRate.FirstOrDefault(x => x.StationNumber == item.StationNumber)?.YieldRate;         
                //稼动时间 = 负荷时间 - 停机损失时间
                var operatingTime = loadTime - downtimeLossTime / 60; // 转换为分钟
                //时间开动率 = 稼动时间 / 负荷时间
                var TimeActivationRate = operatingTime / loadTime;
                var cycleTimeMinutes = (item.CycleTime ?? 0) / 60;
                //性能开动率 = 理论节拍 * 产量 / 稼动时间
                var performanceActivationRate = ((double)cycleTimeMinutes * totalQuantity) / operatingTime;
                // OEE = 时间开动率 * 性能开动率 * 合格率
                var oeeRate = TimeActivationRate * performanceActivationRate * ((yieldRateItem / 100) ?? 0);

                var oee = new EquimentOEE
                {
                    StationNumber = item.StationNumber,
                    StationDescription = item.Name, 
                    Availability = Math.Round(oeeRate * 100, 2) 
                };

                equiments.Add(oee);
            });

            return equiments;
        }

        public async Task<string> UploadAndonAsync(AndonInformation andon)
        {
            await _mediator.Publish(new AndonInformationNotification { AndonData = andon });
            return "Andon information uploaded successfully.";
        }

        /// <summary>
        /// 获取产线工站
        /// </summary>
        /// <param name="lineName"></param>
        /// <returns></returns>
        private async Task<List<int>?> GetStationAsync(string lineName)
        {
            var factory = CacheFactory.Create(lineName, _dataQueryService.LoadStationsByLineAsync);

            var stationList = await _redisCache.GetOrSetAsync("stations", factory, TimeSpan.FromMinutes(10));

            return stationList;
        }
      
    }
}
