// Caso de uso de consulta de estatísticas: lê os metadados da URL sem rastreamento e sem incrementar cliques.
using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services;

public class GetUrlStatsService(AppDbContext dbContext)
{
    public async Task<Result<UrlStatsResponse>> ExecuteAsync(
        string shortCode,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        // AsNoTracking evita o custo do Change Tracker porque essa consulta é apenas de leitura.
        var entity = await dbContext.ShortenedUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode, cancellationToken);

        return entity is null
            ? Result<UrlStatsResponse>.Failure(ErrorType.NotFound, $"Short code '{shortCode}' was not found.")
            : Result<UrlStatsResponse>.Success(entity.ToStatsResponse(baseUrl));
    }
}
