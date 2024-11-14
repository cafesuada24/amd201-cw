using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Databases;
using UrlShortener.Service;
using StackExchange.Redis;

// namespace UrlShortener.Web.Extensions;
namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions {

    	/// <summary>
    	/// Add needed instances for database
    	/// </summary>
    	/// <param name="services"></param>
    	/// <param name="configuration"></param>
    	/// <returns></returns>
    	public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration) {

            services.AddDbContext<ShortUrlDbContext>(options => {
                options.UseSqlite(
                    configuration.GetConnectionString("ShortenedUrlContext") ?? throw new InvalidOperationException("Connection string 'ShortenedUrlContext' not found."),
                    sqliteOptions => sqliteOptions.CommandTimeout(128));
            });

            services.AddScoped<Func<ShortUrlDbContext>>((provider) => () => provider.GetService<ShortUrlDbContext>());
            services.AddScoped<DbFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

    	/// <summary>
    	/// Add instances of in-use services
    	/// </summary>
    	/// <param name="services"></param>
    	/// <param name="configuration"></param>
    	/// <returns></returns>
    	public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration) {
			var redisConnStr = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddStackExchangeRedisCache(options => {
				options.Configuration = redisConnStr;
				// options.InstanceName = "ShortenedUrlCache:";
		});
			services.AddSingleton<IConnectionMultiplexer>(sp =>
				ConnectionMultiplexer.Connect(redisConnStr));

            services.AddScoped<IShortenedUrlService, ShortenedUrlService>();
			services.AddScoped<IShortenedUrlCacheService, RedisShortenedUrlCacheService>();
			services.AddHostedService<ShortenedUrlCacheRefreshService>();

			return services;
        }
}