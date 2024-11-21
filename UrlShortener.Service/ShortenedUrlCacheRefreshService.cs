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
    // private readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

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

                await cacheService.RefreshTopUrlsAsync(topAccessUrls);
            }
            await Task.Delay(RefreshInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        // _logger.LogInformation("Cache to Database Service is stopping and flushing cache to database.");
        await UpdateCacheToDatabaseAsync();
        await ClearCacheAsync();

        await base.StopAsync(stoppingToken);
    }

    private async Task UpdateCacheToDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cacheService = scope.ServiceProvider.GetRequiredService<IShortenedUrlCacheService>();

        var cachedData = await cacheService.GetAllAsync();
        var repo = unitOfWork.Repository<ShortenedUrl>();

        foreach (var data in cachedData)
        {
            try
            {
                var entity = repo.Entities.Where(e => e.Id == data.Id).SingleOrDefault();

                if (entity == null)
                {
                    continue;
                }

                entity.ClickCount = data.ClickCount;
                if (data.LastAccessTime > -1)
                {
                    entity.LastAccessTime = new DateTime(data.LastAccessTime);
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Error occured when pulling entities from cache to database");
            }
        }
    }

    private async Task ClearCacheAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var cacherService = scope.ServiceProvider.GetRequiredService<IShortenedUrlCacheService>();
        await cacherService.ClearCacheAsync();
    }
}