using Application.Interfaces;
using Contracts;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DataQueryService : IDataQueryService
    {
        private readonly MESDBContext _db;

        public DataQueryService(MESDBContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public async Task<List<int>?> LoadStationsByLineAsync(string lineName)
        {          
            var stationList = await _db.Cell_Stations
                        .Join(_db.Stations, a => a.StationId, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Cells, ab => ab.a.CellId, c => c.Id, (ab, c) => new { ab, c })
                        .Where(x => x.c.CellNumber == lineName)
                        .Select(x => x.ab.b.Id)
                        .ToListAsync();

            return stationList;
        }
    }
}
