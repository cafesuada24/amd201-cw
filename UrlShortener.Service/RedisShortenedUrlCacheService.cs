using System.Text.Json;
using StackExchange.Redis;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Service;

public class RedisShortenedUrlCacheService : IShortenedUrlCacheService 
{
    private const string ShortUrlKeyPrefix = "url_short:";
    private const string IdKeyPrefix = "url_id:";
    private const string ClickCountSet = "topurls";
    private const string IdToShortUrlHash = "idtoshorturl";
    private const string ShortUrlToObjectHash = "shorttoobject";
    private readonly IDatabase _cache;

    public RedisShortenedUrlCacheService(IConnectionMultiplexer connectionMultiplexer) {
        _cache = connectionMultiplexer.GetDatabase();
    }

    public async Task AddUrlAsync(ShortenedUrl url)
    {
        var serializedData = JsonSerializer.Serialize(url);
        var idKey = $"{IdKeyPrefix}{url.Id}";
        var shortUrlKey = $"{ShortUrlKeyPrefix}{url.ShortCode}";

        await _cache.HashSetAsync(IdToShortUrlHash, idKey, shortUrlKey);
        await _cache.HashSetAsync(ShortUrlToObjectHash, shortUrlKey, serializedData);
        await _cache.SortedSetAddAsync(ClickCountSet, shortUrlKey, url.ClickCount);
    }

    public async Task<ShortenedUrl?> GetFromShortCodeAsync(string shortCode, bool isPrefixIncluded=false)
    {
        var shortUrlKey = $"{(!isPrefixIncluded ? ShortUrlKeyPrefix : "")}{shortCode}";

        var serializedData = await _cache.HashGetAsync(ShortUrlToObjectHash, shortUrlKey);
        return serializedData.HasValue ? JsonSerializer.Deserialize<ShortenedUrl>(serializedData.ToString()) : null;
    }

    public async Task<ShortenedUrl?> GetFromUrlIdAsync(int urlId, bool isPrefixIncluded=false)
    {
        var idKey = $"{(!isPrefixIncluded ? IdKeyPrefix : "")}{urlId}";
        var shortUrlKey = await _cache.HashGetAsync(IdToShortUrlHash, idKey);
        return shortUrlKey.HasValue ? await GetFromShortCodeAsync(shortUrlKey.ToString()) : null;
    }

    public async Task<List<ShortenedUrl>> GetTopUrlsAsync(int topN)
    {
        var topUrlKeys = await _cache.SortedSetRangeByScoreAsync(ClickCountSet, 0, double.MaxValue, Exclude.None, Order.Descending, 0, topN);
        var urls = new List<ShortenedUrl>(topUrlKeys.Length);
        foreach (var shortUrl in topUrlKeys) {
            if (shortUrl.IsNull) {
                continue;
            }
            var url = await GetFromShortCodeAsync(shortUrl.ToString(), true);
            if (url != null) {
                urls.Add(url);
            }
        }
        return urls;
    }

    public async Task RefreshCacheAsync(List<ShortenedUrl> urls)
    {
        await _cache.KeyDeleteAsync(IdToShortUrlHash);
        await _cache.KeyDeleteAsync(ShortUrlToObjectHash);
        await _cache.KeyDeleteAsync(ClickCountSet);

        foreach (var url in urls) {
            await AddUrlAsync(url);
        }
    }
}