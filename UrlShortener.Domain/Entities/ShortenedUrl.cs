# nullable disable

namespace UrlShortener.Domain.Entities;

public class ShortenedUrl : EntityBase<int>
{
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public int ClickCount { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public int Status { get; set; }
}