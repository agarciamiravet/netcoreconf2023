using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NetCoreConfMad2023.Observability.Metrics;
using NetCoreConfMad2023_API.Models;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreConfMad2023_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly NetCoreConf2023Metrics _meters;
        private readonly IDistributedCache _redisCache;

        public ShoppingCartController(IDistributedCache redisCache,  NetCoreConf2023Metrics meters)
        {
            _redisCache = redisCache;
            _meters = meters;
        }

        // GET api/<ShoppingCartController>/5
        [HttpGet("{username}")]
        public async Task<ShoppingCart> GetBasket(string username)
        {
            var basket = await _redisCache.GetStringAsync(username);

            if (string.IsNullOrEmpty(basket))
                return null;

            _meters.AddShoppingCartView();

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }
   
        // POST api/<ShoppingCartController>
        [HttpPost]
        public async Task UpdateBasket([FromBody] ShoppingCart basket)
        {

            await _redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));
            _meters.AddShoppingCartProductAdded();
        }
    }
}
