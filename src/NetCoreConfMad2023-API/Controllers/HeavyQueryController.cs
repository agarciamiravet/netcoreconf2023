using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using NetCoreConfMad2023.Observability.Metrics;
using NetCoreConfMad2023_API.Data;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace NetCoreConfMad2023_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeavyQueryController : ControllerBase
    {
        private readonly OnlineShoppingContext _context;

        public HeavyQueryController(OnlineShoppingContext context, NetCoreConf2023Metrics meters)
        {
            _context = context;
        }

        [HttpPost]
        public async Task LaunchHeavyQuery()
        {
            var query = "EXEC HighCPU;";
             _context.Database.ExecuteSqlRaw(query);
        }
    }
}
