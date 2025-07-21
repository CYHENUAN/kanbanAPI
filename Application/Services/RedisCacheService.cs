using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RedisCacheService
    {
        private static Lazy<ConnectionMultiplexer>? _lazyConn;

        private readonly StackExchange.Redis.IDatabase _db;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IConfiguration configuration)
        {
            string redisConnection = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string is missing.");
            // 初始化 Lazy 连接
            if (_lazyConn == null || !_lazyConn.IsValueCreated)
            {
                _lazyConn = new Lazy<ConnectionMultiplexer>(() =>
                    ConnectionMultiplexer.Connect(redisConnection));
            }
            _db = _lazyConn.Value.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 可选：驼峰命名
                WriteIndented = false
            };
        }

        // 设置缓存（异步）
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _db.StringSetAsync(key, json, expiry);
        }

        // 获取缓存（异步）+ 滑动过期机制
        public async Task<T?> GetAsync<T>(string key, TimeSpan? slidingExpire = null)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;

            if (!value.IsNull)
            {
                if (slidingExpire != null)
                {
                    await _db.KeyExpireAsync(key, slidingExpire);
                }

                return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            }
            return default;
        }
        // 删除
        public Task<bool> RemoveAsync(string key)
        {
            return _db.KeyDeleteAsync(key);
        }

        // 判断是否存在
        public Task<bool> ExistsAsync(string key)
        {
            return _db.KeyExistsAsync(key);
        }

        // 设置过期时间
        public Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return _db.KeyExpireAsync(key, expiry);
        }

        // 获取 TTL 剩余时间
        public Task<TimeSpan?> GetTTLAsync(string key)
        {
            return _db.KeyTimeToLiveAsync(key);
        }

        // 设置缓存（空值防穿透）
        public async Task SetWithNullProtectionAsync<T>(string key, T value, TimeSpan expiry)
        {
            if (value == null)
            {
                // 标记空值占位符（避免反复查库）
                await _db.StringSetAsync(key, "__null__", expiry);
            }
            else
            {
                await SetAsync(key, value, expiry);
            }
        }

        // 获取缓存（含 null 占位判断）
        public async Task<T?> GetWithNullProtectionAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty || value == "__null__") return default;

            // Ensure the value is not null before deserialization
            if (!value.IsNull)
            {
                return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            }

            return default;
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiry, bool enableNullProtection = true)
        {
            var cached = await _db.StringGetAsync(key);
            if (!cached.IsNullOrEmpty)
            {
                if (enableNullProtection && cached == "__null__")
                    return default;
                if (!cached.IsNull)
                {
                    // 如果有缓存，更新滑动过期时间
                    await _db.KeyExpireAsync(key, expiry);
                    return JsonSerializer.Deserialize<T>(cached.ToString(), _jsonOptions);
                }
            }

            var result = await factory();

            if (result == null && enableNullProtection)
            {
                await _db.StringSetAsync(key, "__null__", expiry);
            }
            else
            {
                var json = JsonSerializer.Serialize(result, _jsonOptions);
                await _db.StringSetAsync(key, json, expiry);
            }

            return result;
        }
    }
}
