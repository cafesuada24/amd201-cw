using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Services.ShortenedUrlsCache;
using UrlShortener.Domain.Models;

namespace UrlShortener.Service;

public class RedisShortenedUrlCacheService : IShortenedUrlCacheService
{
    private const string ShortUrlKeyPrefix = "url_short:";
    private const string IdKeyPrefix = "url_id:";
    private const string ClickCountSet = "topurls";
    private const string IdToShortUrlHash = "idtoshorturl";
    private const string ShortUrlToObjectHash = "shorttoobject";
    private const string ShortUrlToOriginalHash = "shorttooriginal";
    private readonly IDatabase _cache;
    private IConnectionMultiplexer _connectionMultiplexer;

    public RedisShortenedUrlCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _cache = connectionMultiplexer.GetDatabase();
    }

    public async Task AddUrlAsync(ShortenedUrlCacheItem item)
    {
        var shortUrlKey = $"{ShortUrlKeyPrefix}{item.ShortCode}";

        await SetUrlCache(item, shortUrlKey);
        await _cache.SortedSetAddAsync(ClickCountSet, item.ShortCode, item.ClickCount);
    }

    public async Task<ShortenedUrlCacheItem?> GetFromShortCodeAsync(string shortCode)
    {
        var shortUrlKey = $"{ShortUrlKeyPrefix}{shortCode}";
        return await GetFromKey(shortUrlKey);
    }

    public async Task<List<ShortenedUrlCacheItem>> GetTopUrlsAsync(int topN)
    {
        var topUrlKeys = await _cache.SortedSetRangeByScoreAsync(ClickCountSet, 0, double.MaxValue, Exclude.None, Order.Descending, 0, topN);
        var urls = new List<ShortenedUrlCacheItem>(topUrlKeys.Length);
        foreach (var shortUrl in topUrlKeys)
        {
            if (shortUrl.IsNull)
            {
                continue;
            }
            var url = await GetFromShortCodeAsync(shortUrl.ToString());

            if (url != null)
            {
                urls.Add(url);
            }
        }
        return urls;
    }

    public async Task RefreshTopUrlsAsync(IList<ShortenedUrlCacheItem> urls)
    {
        await ClearCacheAsync();

        foreach (var url in urls)
        {
            await AddUrlAsync(url);
        }
    }

    public async Task<bool> TryUpdateAccessStatus(string shortCode)
    {
        var item = await GetFromShortCodeAsync(shortCode);
        if (item == null)
        {
            return false;
        }
        item.ClickCount += 1;
        item.LastAccessTime = DateTime.Now.Ticks;


        var shortUrlKey = $"{ShortUrlKeyPrefix}{shortCode}";
        await SetUrlCache(item, shortUrlKey);
        return true;
    }

    public async Task ClearCacheAsync()
    {
        var keys = GetKeys(ShortUrlKeyPrefix);
        await _cache.KeyDeleteAsync(keys);
    }

    public async Task<IList<ShortenedUrlCacheItem>> GetAllAsync()
    {
        var keys = GetKeys(ShortUrlKeyPrefix);
        IList<ShortenedUrlCacheItem> items = [];
        foreach (var key in keys)
        {
            var value = await GetFromKey(key.ToString());
            if (value != null)
            {
                items.Add(value);
            }
        }
        return items;
    }

    private RedisKey[] GetKeys(string pattern)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
        // var server = _connectionMultiplexer.GetServer("host");
        return server.Keys(pattern: pattern + "*").ToArray();
    }

    private async Task<ShortenedUrlCacheItem?> GetFromKey(string key)
    {
        if (!await IsValidUrlCache(key))
        {
            return null;
        }
        return new()
        {
            Id = (int)await _cache.HashGetAsync(key, "ID"),
            OriginalUrl = await _cache.HashGetAsync(key, "OriginalUrl"),
            ShortCode = await _cache.HashGetAsync(key, "ShortCode"),
            ClickCount = (int)await _cache.HashGetAsync(key, "ClickCount"),
            LastAccessTime = (long)await _cache.HashGetAsync(key, "LastAccessTime")
        };
    }

    private async Task SetUrlCache(ShortenedUrlCacheItem cacheItem, RedisKey key)
        => await _cache.HashSetAsync(key, ConvertToHashEntries(cacheItem));

    private async Task<bool> IsValidUrlCache(string key)
        => await _cache.HashExistsAsync(key, "OriginalUrl") &&
            await _cache.HashExistsAsync(key, "ShortCode") &&
            await _cache.HashExistsAsync(key, "ID") &&
            await _cache.HashExistsAsync(key, "ClickCount") &&
            await _cache.HashExistsAsync(key, "LastAccessTime");

    private static HashEntry[] ConvertToHashEntries(ShortenedUrlCacheItem item)
        => [
            new ("ID", item.Id),
            new ("OriginalUrl", item.OriginalUrl),
            new ("ShortCode", item.ShortCode),
            new ("ClickCount", item.ClickCount),
            new ("LastAccessTime", item.LastAccessTime)
        ];
}