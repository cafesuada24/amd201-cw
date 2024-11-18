namespace UrlShortener.Domain.Interfaces.Services.ShortenedUrls;

public interface IShortenedUrlRead
{
    public Task<string?> GetOriginalUrlAsync(string shortCode);
}