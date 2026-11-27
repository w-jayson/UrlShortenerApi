// Caso de uso de criação de URL encurtada: valida a entrada, gera o shortCode e persiste com retry em colisões.
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services;

public class CreateShortUrlService(AppDbContext dbContext, ShortCodeGenerator codeGenerator)
{
    private const int MaxCodeGenerationAttempts = 5;

    public async Task<Result<ShortUrlResponse>> ExecuteAsync(
        string originalUrl,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var validationResult = UrlValidator.Validate(originalUrl);
        if (!validationResult.IsSuccess)
        {
            return Result<ShortUrlResponse>.Failure(
                validationResult.ErrorType.Value,
                validationResult.ErrorMessage);
        }

        // Tenta gerar e salvar um código único até o limite configurado caso ocorra colisão no índice UNIQUE.
        for (var attempt = 1; attempt <= MaxCodeGenerationAttempts; attempt++)
        {
            var entity = new ShortenedUrl
            {
                OriginalUrl = validationResult.Value,
                ShortCode = codeGenerator.Generate(),
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.ShortenedUrls.Add(entity);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<ShortUrlResponse>.Success(entity.ToShortUrlResponse(baseUrl));
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Desanexa a entidade que falhou para que o Change Tracker do EF Core não tente inseri-la novamente no próximo retry.
                dbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        return Result<ShortUrlResponse>.Failure(
            ErrorType.Unexpected,
            "Failed to generate a unique short code. Please try again.");
    }

    // Código 19 no SQLite representa SQLITE_CONSTRAINT (violação de restrição, como índice UNIQUE).
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqliteException { SqliteErrorCode: 19 };
}
