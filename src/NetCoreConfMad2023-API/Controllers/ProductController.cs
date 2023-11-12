using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetCoreConfMad2023.Observability.Metrics;
using NetCoreConfMad2023_API.Data;
using NetCoreConfMad2023_API.Models;

namespace NetCoreConfMad2023_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly OnlineShoppingContext _context;

        private readonly NetCoreConf2023Metrics _meters;

        public ProductController(OnlineShoppingContext context, NetCoreConf2023Metrics meters)
        {
            _context = context;
            _meters = meters;
        }

        // GET: Products
        [HttpGet()]
        public async Task<IEnumerable<Product>> Get()
        {
            _meters.AddViewCatalog();

            var result = await _context.Products.ToListAsync();

            return result;
        }

        // GET: Products
        [HttpGet("{id}")]
        public async Task<Product> Get(string id)
        {
            var product = new Product();
            var guidNew = new Guid(id);

            var result = await _context.Products.Where(product => product.Id == guidNew).FirstOrDefaultAsync();

            return result;
        }
    }
}
