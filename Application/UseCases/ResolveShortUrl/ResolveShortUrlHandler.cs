using UrlShortenerApi.Application.Abstractions;

namespace UrlShortenerApi.Application.UseCases.ResolveShortUrl;

public sealed class ResolveShortUrlHandler
{
    private readonly IShortenedUrlRepository _repository;
    private readonly IBase62Service _base62Service;

    public ResolveShortUrlHandler(IShortenedUrlRepository repository, IBase62Service base62Service)
    {
        _repository = repository;
        _base62Service = base62Service;
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken ct = default)
    {
        var id = _base62Service.Decode(code);
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity?.OriginalUrl;
    }
}
