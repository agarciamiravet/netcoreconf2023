using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreConfMad2023.Observability.Extensions
{
    public static class RedisCacheExtensions
    {
        public static ConnectionMultiplexer GetConnection(this RedisCache cache)
        {
            //ensure connection is established
            typeof(RedisCache).InvokeMember("Connect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, cache, new object[] { });

            //get connection multiplexer
            var fi = typeof(RedisCache).GetField("_connection", BindingFlags.Instance | BindingFlags.NonPublic);
            var connection = (ConnectionMultiplexer)fi.GetValue(cache);
            return connection;
        }
    }
}
