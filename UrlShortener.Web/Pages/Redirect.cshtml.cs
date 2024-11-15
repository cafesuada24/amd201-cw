using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Web.Pages;

public class RedirectModel : PageModel
{
    private IRedirectionService _redirectionService;
    public RedirectModel(IRedirectionService redirectionService)
    {
        _redirectionService = redirectionService;
    }

    public async Task<IActionResult> OnGetAsync(string shortCode)
    {
        string? url = await _redirectionService.GetOriginalUrlAsync(shortCode);
        if (url == null)
        {
            return NotFound("Invalid short url");
        }

        return Redirect(url);
    }
}