using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Service;

public class ShortenedUrlCacheRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(10);

    public ShortenedUrlCacheRefreshService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cacheService = scope.ServiceProvider.GetRequiredService<IShortenedUrlCacheService>();
                var urlService = scope.ServiceProvider.GetRequiredService<IShortenedUrlService>();
                await cacheService.RefreshCacheAsync([.. urlService.GetEntities().OrderByDescending(static x => x.ClickCount).Take(10)]);
            }
            await Task.Delay(RefreshInterval, stoppingToken);
        }
    }
}