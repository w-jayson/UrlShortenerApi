// Métodos de extensão para converter a entidade ShortenedUrl nos DTOs de resposta da API.
namespace UrlShortenerApi.Models;

public static class ShortenedUrlMappings
{
    public static ShortUrlResponse ToShortUrlResponse(this ShortenedUrl entity, string baseUrl) =>
        new(
            entity.ShortCode,
            BuildShortUrl(baseUrl, entity.ShortCode),
            entity.OriginalUrl,
            entity.CreatedAtUtc);

    public static UrlStatsResponse ToStatsResponse(this ShortenedUrl entity, string baseUrl) =>
        new(
            entity.ShortCode,
            BuildShortUrl(baseUrl, entity.ShortCode),
            entity.OriginalUrl,
            entity.ClickCount,
            entity.CreatedAtUtc,
            entity.LastAccessedAtUtc);

    // Normaliza a barra final da baseUrl para evitar URLs com "//" ao montar o link completo.
    private static string BuildShortUrl(string baseUrl, string shortCode) =>
        $"{baseUrl.TrimEnd('/')}/{shortCode}";
}
