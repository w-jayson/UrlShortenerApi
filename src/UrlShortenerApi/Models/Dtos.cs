using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UrlShortenerApi.Models;

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

public enum ErrorType
{
    Validation,
    NotFound,
    Unexpected
}

public sealed record Result<T>
{
    private Result(T? value, ErrorType? errorType, string? errorMessage)
    {
        Value = value;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }
    public ErrorType? ErrorType { get; }
    public string? ErrorMessage { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(ErrorType), nameof(ErrorMessage))]
    public bool IsSuccess => ErrorType is null;

    public static Result<T> Success(T value) => new(value, null, null);

    public static Result<T> Failure(ErrorType errorType, string errorMessage) =>
        new(default, errorType, errorMessage);
}

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

    private static string BuildShortUrl(string baseUrl, string shortCode) =>
        $"{baseUrl.TrimEnd('/')}/{shortCode}";
}
