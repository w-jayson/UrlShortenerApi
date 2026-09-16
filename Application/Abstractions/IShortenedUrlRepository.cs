using UrlShortenerApi.Domain.Entities;

namespace UrlShortenerApi.Application.Abstractions;

public interface IShortenedUrlRepository
{
    Task<int> AddAsync(ShortenedUrl entity, CancellationToken ct = default);
    Task<ShortenedUrl?> GetByIdAsync(int id, CancellationToken ct = default);
}
