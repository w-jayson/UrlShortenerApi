using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Domain.Entities;

namespace UrlShortenerApi.Infrastructure.Persistence;

public sealed class ShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly AppDbContext _context;

    public ShortenedUrlRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddAsync(ShortenedUrl entity, CancellationToken ct = default)
    {
        _context.ShortenedUrls.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<ShortenedUrl?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ShortenedUrls.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
