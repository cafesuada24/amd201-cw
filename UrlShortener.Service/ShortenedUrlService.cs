using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Service;

public class ShortenedUrlService(IUnitOfWork unitOfWork, IShortenedUrlCacheService shortenedUrlCacheService) : IShortenedUrlService
{
    private const string ShortUrlFormat = "http://localhost:5162/redirect/{0}";
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IShortenedUrlCacheService _shortenedUrlCacheService = shortenedUrlCacheService;

    public async Task Add(ShortenedUrl shortenedUrl)
    {
        string shortCode = GenerateShortCode(shortenedUrl.OriginalUrl.ToString());
        // string shortUrlStr = $"http://shortedurl.he/{shortCode}";

        shortenedUrl.ShortCode = shortCode;
        shortenedUrl.ClickCount = 0;
        shortenedUrl.CreatedAt = DateTime.Now;
        shortenedUrl.ExpireAt = null;
        shortenedUrl.LastAccessTime = null;
        shortenedUrl.Status = 0;

        await _unitOfWork.Repository<ShortenedUrl>().InsertAsync(shortenedUrl);
    }

    public async Task<ShortenedUrl> Add(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var orignalUrl) || orignalUrl == null)
        {
            throw new ArgumentException("Invalid url format");
        }
        ShortenedUrl shortenedUrl = new() { OriginalUrl = url };

        await Add(shortenedUrl);

        return shortenedUrl;
    }

    public async Task Delete(ShortenedUrl shortenedUrl)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<ShortenedUrl>> GetAll()
    {
        return await _unitOfWork.Repository<ShortenedUrl>().GetAllAsync();
    }

    public DbSet<ShortenedUrl> GetEntities()
    {
        return _unitOfWork.Repository<ShortenedUrl>().Entities;
    }

    public Task<ShortenedUrl> GetOne(int urlId)
    {
        throw new NotImplementedException();
    }

    public async Task Update(ShortenedUrl shortenedUrl)
    {
        throw new NotImplementedException();
    }


    private static string GenerateShortCode(string url)
    {
        int hash = url.GetHashCode();
        return Convert.ToBase64String(BitConverter.GetBytes(hash))
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var repo = _unitOfWork.Repository<ShortenedUrl>();
        var shortenedUrl =
            await _shortenedUrlCacheService.GetFromShortCodeAsync(shortCode, false) ??
            await repo.Entities.Where(
                    x => x.ShortCode == shortCode
                ).SingleAsync();
        if (shortenedUrl != null)
        {
            repo.DbContext.Attach(shortenedUrl);
            shortenedUrl.LastAccessTime = DateTime.Now;
            shortenedUrl.ClickCount = shortenedUrl.ClickCount + 1;
            await _unitOfWork.SaveChangesAsync();
        }
        return shortenedUrl?.OriginalUrl;
    }
}