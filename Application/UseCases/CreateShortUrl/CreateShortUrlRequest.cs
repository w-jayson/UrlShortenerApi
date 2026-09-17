namespace UrlShortenerApi.Application.UseCases.CreateShortUrl;

public record CreateShortUrlRequest(string Url, string? CustomSlug = null);
