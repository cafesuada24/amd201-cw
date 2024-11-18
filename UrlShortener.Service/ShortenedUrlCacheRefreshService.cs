using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services.ShortenedUrlsCache;
using UrlShortener.Domain.Models;

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
                // var urlService = scope.ServiceProvider.GetRequiredService<IShortenedUrlService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var topAccessUrls = await
                    unitOfWork.Repository<ShortenedUrl>()
                    .Entities
                    .OrderByDescending(static x => x.ClickCount)
                    .Take(10)
                    .Select(static x => ShortenedUrlCacheItem.FromEntity(x))
                    .ToListAsync(stoppingToken);

                // foreach (var item in items)
                // {
                //     Console.WriteLine(item);
                // }

                // await cacheService.RefreshTopUrlsAsync(await
                //     unitOfWork.Repository<ShortenedUrl>()
                //     .Entities
                //     .OrderByDescending(static x => x.ClickCount)
                //     .Take(10)
                //     .Select(static x => ShortenedUrlCacheItem.FromEntity(x))
                //     .ToListAsync(stoppingToken)
                // );
                await cacheService.RefreshTopUrlsAsync(topAccessUrls);
            }
            await Task.Delay(RefreshInterval, stoppingToken);
        }
    }
}