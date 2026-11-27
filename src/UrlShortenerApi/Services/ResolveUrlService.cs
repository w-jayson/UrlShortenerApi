// Caso de uso de redirecionamento: incrementa cliques atomicamente no banco e retorna a URL original.
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

        // ExecuteUpdateAsync gera um UPDATE direto no SQLite (ClickCount + 1), evitando perda de cliques em acessos concorrentes.
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

        // Busca apenas a URL original sem rastreamento do EF Core (somente leitura).
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
