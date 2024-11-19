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
        // var serializedData = JsonSerializer.Serialize(url);
        // var idKey = $"{IdKeyPrefix}{url.Id}";
        var shortUrlKey = $"{ShortUrlKeyPrefix}{item.ShortCode}";

        // await _cache.HashSetAsync(IdToShortUrlHash, idKey, shortUrlKey);
        // await _cache.HashSetAsync(ShortUrlToObjectHash, shortUrlKey, serializedData);
        // var entries = ConvertToHashEntries(item);
        await SetUrlCache(item, shortUrlKey);
        await _cache.SortedSetAddAsync(ClickCountSet, item.ShortCode, item.ClickCount);
    }

    public async Task<ShortenedUrlCacheItem?> GetFromShortCodeAsync(string shortCode)
    {
        var shortUrlKey = $"{ShortUrlKeyPrefix}{shortCode}";
        if (!await IsValidUrlCache(shortUrlKey))
        {
            return null;
        }
        return new()
        {
            OriginalUrl = await _cache.HashGetAsync(shortUrlKey, "OriginalUrl"),
            ShortCode = shortCode,
            ClickCount = (int)await _cache.HashGetAsync(shortUrlKey, "ClickCount"),
            LastAccessTime = (long)await _cache.HashGetAsync(shortUrlKey, "LastAccessTime")
        };
        // return new() { ShortCode = serializedData.ToString() };
        // return serializedData.HasValue ? JsonSerializer.Deserialize<ShortenedUrl>(serializedData.ToString()) : null;
    }

    // public async Task<ShortenedUrl?> GetFromUrlIdAsync(int urlId, bool isPrefixIncluded = false)
    // {
    //     var idKey = $"{(!isPrefixIncluded ? IdKeyPrefix : "")}{urlId}";
    //     var shortUrlKey = await _cache.HashGetAsync(IdToShortUrlHash, idKey);
    //     return shortUrlKey.HasValue ? await GetFromShortCodeAsync(shortUrlKey.ToString()) : null;
    // }

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

    // public async Task RefreshTopUrlsAsync(List<ShortenedUrlCacheItem> urls)
    // {
    //     await _cache.KeyDeleteAsync(IdToShortUrlHash);
    //     await _cache.KeyDeleteAsync(ShortUrlToObjectHash);
    //     await _cache.KeyDeleteAsync(ClickCountSet);

    //     foreach (var url in urls)
    //     {
    //         await AddUrlAsync(url);
    //     }
    // }

    private static HashEntry[] ConvertToHashEntries(ShortenedUrlCacheItem item)
        => [
            new ("OriginalUrl", item.OriginalUrl),
            new ("ClickCount", item.ClickCount),
            new ("LastAccessTime", item.LastAccessTime)
        ];

    public async Task RefreshTopUrlsAsync(IList<ShortenedUrlCacheItem> urls)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: ShortUrlKeyPrefix).ToArray();
        await _cache.KeyDeleteAsync(keys);

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

    private async Task SetUrlCache(ShortenedUrlCacheItem cacheItem, string key)
        => await _cache.HashSetAsync(key, ConvertToHashEntries(cacheItem));

    private async Task<bool> IsValidUrlCache(string key)
        => await _cache.HashExistsAsync(key, "OriginalUrl") &
            await _cache.HashExistsAsync(key, "ClickCount") &&
            await _cache.HashExistsAsync(key, "LastAccessTime");
}