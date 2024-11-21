using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services.ShortenedUrls;
using UrlShortener.Domain.Interfaces.Services.ShortenedUrlsCache;

namespace UrlShortener.Service;

public class ShortenedUrlService(IUnitOfWork unitOfWork, IShortenedUrlCacheService shortenedUrlCacheService) : IShortenedUrlService
{
    private const string Base62Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IShortenedUrlCacheService _shortenedUrlCacheService = shortenedUrlCacheService;

    // public async Task Add(ShortenedUrl shortenedUrl)
    // {
    //     string shortCode = GenerateShortCode(shortenedUrl.OriginalUrl.ToString());
    //     // string shortUrlStr = $"http://shortedurl.he/{shortCode}";

    //     shortenedUrl.ShortCode = shortCode;
    //     shortenedUrl.ClickCount = 0;
    //     shortenedUrl.CreatedAt = DateTime.Now;
    //     shortenedUrl.ExpireAt = null;
    //     shortenedUrl.LastAccessTime = null;
    //     shortenedUrl.Status = 0;

    //     await _unitOfWork.Repository<ShortenedUrl>().InsertAsync(shortenedUrl);
    // }

    public async Task<ShortenedUrl> CreateShortUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var orignalUrl) || orignalUrl == null)
        {
            throw new ArgumentException("Invalid url format");
        }


        int increment = 0;
        var time = DateTime.Now;
        // do
        // {
        try
        {
            string shortCode = GenerateShortCode(url.Append(increment));
            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = url,
                ShortCode = shortCode,
                ClickCount = 0,
                CreatedAt = time,
                ExpireAt = null,
                LastAccessTime = null,
                Status = 0
            };
            await _unitOfWork.Repository<ShortenedUrl>().InsertAsync(shortenedUrl);
            return shortenedUrl;
        }
        catch (Exception)
        {
            ++increment;
        }

        // } while (true);
        return null;
    }

    public async Task Delete(ShortenedUrl shortenedUrl)
    {
        throw new NotImplementedException();
    }

    // public async Task<IList<ShortenedUrl>> GetAll()
    // {
    //     return await _unitOfWork.Repository<ShortenedUrl>().GetAllAsync();
    // }

    // public DbSet<ShortenedUrl> GetEntities()
    // {
    //     return _unitOfWork.Repository<ShortenedUrl>().Entities;
    // }

    // public Task<ShortenedUrl> GetOne(int urlId)
    // {
    //     throw new NotImplementedException();
    // }

    // public async Task Update(ShortenedUrl shortenedUrl)
    // {
    //     throw new NotImplementedException();
    // }

    private static string GenerateShortCode(string url)
    {
        // int hash = url.GetHashCode();
        // return Convert.ToBase64String(BitConverter.GetBytes(hash))
        //     .Replace("+", "")
        //     .Replace("/", "")
        //     .Replace("=", "");
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("The URL cannot be null or empty.", nameof(url));

        using var md5 = MD5.Create();
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(url));

        ulong hashSegment = BitConverter.ToUInt64(hashBytes, 0) & 0xFFFFFFFFFFFF;

        return Base62Encode(hashSegment);
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var cacheItem = await _shortenedUrlCacheService.GetFromShortCodeAsync(shortCode);
        string? originalUrl = cacheItem?.OriginalUrl;

        if (cacheItem == null)
        {
            var repo = _unitOfWork.Repository<ShortenedUrl>();
            originalUrl = (await repo.Entities.Where(
                    x => x.ShortCode == shortCode
                ).SingleOrDefaultAsync())?.ShortCode.ToString();
        }
        return originalUrl;
    }

    public async Task UpdateAccessStatus(string shortCode)
    {
        if (await _shortenedUrlCacheService.TryUpdateAccessStatus(shortCode))
        {
            return;
        }
        var entity = await _unitOfWork.Repository<ShortenedUrl>().Entities.Where(x => x.ShortCode == shortCode).FirstOrDefaultAsync() ??
            throw new ArgumentException("Invalid short code");

        entity.ClickCount += 1;
        entity.LastAccessTime = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();
    }

    private static string Base62Encode(ulong value)
    {
        var sb = new StringBuilder();
        do
        {
            sb.Insert(0, Base62Characters[(int)(value % 62)]);
            value /= 62;
        } while (value > 0);

        return sb.ToString();
    }

}