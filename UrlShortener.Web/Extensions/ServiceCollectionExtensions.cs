using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Databases;
using UrlShortener.Service;

// namespace UrlShortener.Web.Extensions;
namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions {

    	/// <summary>
    	/// Add needed instances for database
    	/// </summary>
    	/// <param name="services"></param>
    	/// <param name="configuration"></param>
    	/// <returns></returns>
    	public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration) {

            services.AddDbContext<ShortUrlDbContext>(options => {
                options.UseSqlite(
                    configuration.GetConnectionString("UrlShortenedContext") ?? throw new InvalidOperationException("Connection string 'UrlShortenedContext' not found."),
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
    	/// <returns></returns>
    	public static IServiceCollection AddServices(this IServiceCollection services) {
            return services.AddScoped<IShortenedUrlService, ShortenedUrlService>();
        }
}