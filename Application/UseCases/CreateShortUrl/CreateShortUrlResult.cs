namespace UrlShortenerApi.Application.UseCases.CreateShortUrl;

public enum CreateShortUrlStatus
{
    Success,
    InvalidUrl,
    InvalidSlug,
    SlugAlreadyExists,
    CollisionConflict
}

public sealed record CreateShortUrlResult(
    CreateShortUrlStatus Status,
    string? Code = null,
    string? ErrorMessage = null)
{
    public bool IsSuccess => Status == CreateShortUrlStatus.Success;

    public static CreateShortUrlResult Success(string code) =>
        new(CreateShortUrlStatus.Success, Code: code);

    public static CreateShortUrlResult InvalidUrl(string message = "URL inválida.") =>
        new(CreateShortUrlStatus.InvalidUrl, ErrorMessage: message);

    public static CreateShortUrlResult InvalidSlug(string message) =>
        new(CreateShortUrlStatus.InvalidSlug, ErrorMessage: message);

    public static CreateShortUrlResult SlugAlreadyExists(string message = "O slug customizado informado já está em uso.") =>
        new(CreateShortUrlStatus.SlugAlreadyExists, ErrorMessage: message);

    public static CreateShortUrlResult CollisionConflict(string message = "Não foi possível gerar um código único após múltiplas tentativas.") =>
        new(CreateShortUrlStatus.CollisionConflict, ErrorMessage: message);
}
