using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IShortenedUrlService _shortenedUrlService;
    private readonly IShortenedUrlCacheService _cache;

    public IndexModel(ILogger<IndexModel> logger, IShortenedUrlService shortenedUrlService, IShortenedUrlCacheService cache)
    {
        _logger = logger;
        _shortenedUrlService = shortenedUrlService;
        _cache = cache;

    }

    public IList<ShortenedUrl> TopAccessUrls { get; private set; }
    public async Task OnGetAsync()
    {
        await LoadSharedData();
    }

    [BindProperty]
    public string UrlInput { get; set; }
    public ShortenedUrl ShortenedUrl { get; set; }
    public async Task<IActionResult> OnPostAsync()
    {

        await LoadSharedData();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        ShortenedUrl = await _shortenedUrlService.Add(UrlInput);
        return Page();
    }

    private async Task LoadSharedData()
    {

        TopAccessUrls = await _cache.GetTopUrlsAsync(10);
    }
}
