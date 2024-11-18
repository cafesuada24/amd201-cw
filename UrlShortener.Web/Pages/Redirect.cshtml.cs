using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Domain.Interfaces.Services;
using UrlShortener.Domain.Interfaces.Services.ShortenedUrls;

namespace UrlShortener.Web.Pages;

public class RedirectModel : PageModel
{
    private IShortenedUrlService _shortenedUrlService;
    public RedirectModel(IShortenedUrlService shortenedUrlService)
    {
        _shortenedUrlService = shortenedUrlService;
    }

    public async Task<IActionResult> OnGetAsync(string shortCode)
    {
        string? url = await _shortenedUrlService.GetOriginalUrlAsync(shortCode);
        Console.WriteLine(url);
        if (url == null)
        {
            return NotFound("Invalid short url");
        }

        await _shortenedUrlService.UpdateAccessStatus(shortCode);

        return Redirect(url);
    }
}