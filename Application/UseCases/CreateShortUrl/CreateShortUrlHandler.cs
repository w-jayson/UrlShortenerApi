using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Domain.Entities;
using UrlShortenerApi.Domain.Validation;
using UrlShortenerApi.Infrastructure.Persistence;

namespace UrlShortenerApi.Application.UseCases.CreateShortUrl;

public sealed class CreateShortUrlHandler
{
    private const int MaxRetries = 3;
    private readonly AppDbContext _context;
    private readonly IShortCodeGenerator _codeGenerator;

    public CreateShortUrlHandler(AppDbContext context, IShortCodeGenerator codeGenerator)
    {
        _context = context;
        _codeGenerator = codeGenerator;
    }

    public async Task<CreateShortUrlResult> HandleAsync(CreateShortUrlRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Url)
            || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return CreateShortUrlResult.InvalidUrl("URL inválida. A URL deve ser absoluta e utilizar HTTP ou HTTPS.");
        }

        if (request.CustomSlug is not null && string.IsNullOrWhiteSpace(request.CustomSlug))
        {
            return CreateShortUrlResult.InvalidSlug("O slug customizado não pode ser vazio.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomSlug))
        {
            var customSlug = request.CustomSlug.Trim();

            if (!SlugValidator.IsValid(customSlug, out var validationError))
            {
                return CreateShortUrlResult.InvalidSlug(validationError!);
            }

            var entity = new ShortenedUrl
            {
                OriginalUrl = request.Url,
                Code = customSlug,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.ShortenedUrls.Add(entity);

            try
            {
                await _context.SaveChangesAsync(ct);
                return CreateShortUrlResult.Success(entity.Code);
            }
            catch (DbUpdateException)
            {
                _context.Entry(entity).State = EntityState.Detached;
                return CreateShortUrlResult.SlugAlreadyExists();
            }
        }

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var code = _codeGenerator.Generate(7);

            var entity = new ShortenedUrl
            {
                OriginalUrl = request.Url,
                Code = code,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.ShortenedUrls.Add(entity);

            try
            {
                await _context.SaveChangesAsync(ct);
                return CreateShortUrlResult.Success(entity.Code);
            }
            catch (DbUpdateException)
            {
                _context.Entry(entity).State = EntityState.Detached;

                if (attempt == MaxRetries)
                {
                    return CreateShortUrlResult.CollisionConflict();
                }
            }
        }

        return CreateShortUrlResult.CollisionConflict();
    }
}
