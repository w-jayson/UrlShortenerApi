// Valida e normaliza a URL original antes da persistência, permitindo apenas esquemas HTTP/HTTPS.
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services;

public static class UrlValidator
{
    private const int MaxUrlLength = 2048;

    public static Result<string> Validate(string? originalUrl)
    {
        var trimmedUrl = originalUrl?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedUrl))
        {
            return Result<string>.Failure(ErrorType.Validation, "URL is required.");
        }

        if (trimmedUrl.Length > MaxUrlLength)
        {
            return Result<string>.Failure(
                ErrorType.Validation,
                $"URL must not exceed {MaxUrlLength} characters.");
        }

        // Bloqueia URLs relativas e esquemas perigosos (ex.: javascript:, data:, file:) para evitar Open Redirect/XSS.
        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Result<string>.Failure(
                ErrorType.Validation,
                "URL must be an absolute HTTP or HTTPS URL.");
        }

        return Result<string>.Success(trimmedUrl);
    }
}
