using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Service;

public class ShortenedUrlService(IUnitOfWork unitOfWork) : IShortenedUrlService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Add(ShortenedUrl shortenedUrl)
    {
        string shortCode = GenerateShortCode(shortenedUrl.OriginalUrl.ToString());
        string shortUrlStr = $"http://shortedurl.he/{shortCode}";
        Uri shortUrl = new(shortUrlStr, UriKind.Absolute);

        shortenedUrl.ShortUrl = shortUrl;
        shortenedUrl.ClickCount = 0;
        shortenedUrl.CreatedAt = DateTime.Now;
        shortenedUrl.ExpireAt = null;
        shortenedUrl.LastAccessTime = null;
        shortenedUrl.Status = 0;

        await _unitOfWork.Repository<ShortenedUrl>().InsertAsync(shortenedUrl);
    }

    public async Task<ShortenedUrl> Add(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var orignalUrl) || orignalUrl == null) {
            throw new ArgumentException("Invalid url format");
        }
        ShortenedUrl shortenedUrl = new() { OriginalUrl = orignalUrl };

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
}