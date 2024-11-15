
namespace UrlShortener.Domain.Interfaces.Services;

public interface IRedirectionService {
    public Task<string?> GetOriginalUrlAsync(string shortCode);
}