using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services;

public class ResolveUrlService(AppDbContext dbContext)
{
    public async Task<Result<string>> ExecuteAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var rowsAffected = await dbContext.ShortenedUrls
            .Where(u => u.ShortCode == shortCode)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.ClickCount, u => u.ClickCount + 1)
                .SetProperty(u => u.LastAccessedAtUtc, now),
                cancellationToken);

        if (rowsAffected == 0)
        {
            return Result<string>.Failure(
                ErrorType.NotFound,
                $"Short code '{shortCode}' was not found.");
        }

        var originalUrl = await dbContext.ShortenedUrls
            .AsNoTracking()
            .Where(u => u.ShortCode == shortCode)
            .Select(u => u.OriginalUrl)
            .FirstOrDefaultAsync(cancellationToken);

        return originalUrl is null
            ? Result<string>.Failure(ErrorType.NotFound, $"Short code '{shortCode}' was not found.")
            : Result<string>.Success(originalUrl);
    }
}
