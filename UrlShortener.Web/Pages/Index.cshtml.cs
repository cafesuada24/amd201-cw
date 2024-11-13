using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IShortenedUrlService _shortenedUrlService;

    public IList<ShortenedUrl> ShortenedUrls { get; private set; }
    public IndexModel(ILogger<IndexModel> logger, IShortenedUrlService shortenedUrlService)
    {
        _logger = logger;
        _shortenedUrlService =shortenedUrlService;
    }

    public async Task OnGetAsync()
    {
        ShortenedUrls = await _shortenedUrlService.GetAll();
    }

}
