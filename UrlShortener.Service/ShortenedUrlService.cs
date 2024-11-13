using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Services;

namespace UrlShortener.Service;

public class ShortenedUrlService(IUnitOfWork unitOfWork) : IShortenedUrlService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task Add(ShortenedUrl shortenedUrl)
    {

        throw new NotImplementedException();
    }

    public Task Delete(ShortenedUrl shortenedUrl)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<ShortenedUrl>> GetAll()
    {
        return await _unitOfWork.Repository<ShortenedUrl>().GetAllAsync();
    }

    public Task<ShortenedUrl> GetOne(int urlId)
    {
        throw new NotImplementedException();
    }

    public Task Update(ShortenedUrl shortenedUrl)
    {
        throw new NotImplementedException();
    }
}