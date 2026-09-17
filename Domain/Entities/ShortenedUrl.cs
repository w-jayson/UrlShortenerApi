namespace UrlShortenerApi.Domain.Entities;

public class ShortenedUrl
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string OriginalUrl { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
