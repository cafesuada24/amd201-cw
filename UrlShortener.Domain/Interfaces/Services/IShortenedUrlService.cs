using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Services;

public interface IShortenedUrlService : IRedirectionService {
    Task<IList<ShortenedUrl>> GetAll();
    Task<ShortenedUrl> GetOne(int urlId);
    Task Update(ShortenedUrl shortenedUrl);
    Task Add(ShortenedUrl shortenedUrl);
    Task<ShortenedUrl> Add(string url);
    Task Delete(ShortenedUrl shortenedUrl);
    DbSet<ShortenedUrl> GetEntities();
}