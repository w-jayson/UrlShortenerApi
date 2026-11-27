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
                // Detach failed entity so the next retry starts with a clean Change Tracker state.
                dbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        return Result<ShortUrlResponse>.Failure(
            ErrorType.Unexpected,
            "Failed to generate a unique short code. Please try again.");
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqliteException { SqliteErrorCode: 19 };
}
