using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IShortenedUrlService _shortenedUrlService;
    private readonly IShortenedUrlCacheService _cache;

    public IList<ShortenedUrl> ShortenedUrls { get; private set; }
    public IndexModel(ILogger<IndexModel> logger, IShortenedUrlService shortenedUrlService, IShortenedUrlCacheService cache)
    {
        _logger = logger;
        _shortenedUrlService =shortenedUrlService;
        _cache = cache;
    }

    public async Task OnGetAsync()
    {
        ShortenedUrls = await _cache.GetTopUrlsAsync(10);
    }

}
