// Contratos HTTP (entrada e saída) expostos pelos endpoints da API.
using System.ComponentModel.DataAnnotations;

namespace UrlShortenerApi.Models;

// Data Annotations validam os dados na entrada via [ApiController], retornando 400 automaticamente.
public sealed record CreateUrlRequest(
    [property: Required, MaxLength(2048)] string OriginalUrl);

public sealed record ShortUrlResponse(
    string ShortCode,
    string ShortUrl,
    string OriginalUrl,
    DateTime CreatedAtUtc);

public sealed record UrlStatsResponse(
    string ShortCode,
    string ShortUrl,
    string OriginalUrl,
    long ClickCount,
    DateTime CreatedAtUtc,
    DateTime? LastAccessedAtUtc);
