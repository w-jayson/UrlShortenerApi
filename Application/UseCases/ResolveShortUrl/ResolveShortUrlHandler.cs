using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Infrastructure.Persistence;

namespace UrlShortenerApi.Application.UseCases.ResolveShortUrl;

public sealed class ResolveShortUrlHandler
{
    private readonly AppDbContext _context;

    public ResolveShortUrlHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await _context.ShortenedUrls
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => x.OriginalUrl)
            .FirstOrDefaultAsync(ct);
    }
}
