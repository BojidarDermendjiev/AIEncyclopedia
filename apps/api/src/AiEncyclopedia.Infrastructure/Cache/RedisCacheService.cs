using System.Text.Json;
using AiEncyclopedia.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AiEncyclopedia.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get failed for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await _db.KeyDeleteAsync(key); }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache remove failed for key {Key}", key); }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{pattern}*").ToArray();
            if (keys.Length > 0)
                await _db.KeyDeleteAsync(keys);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache pattern remove failed for {Pattern}", pattern); }
    }
}
