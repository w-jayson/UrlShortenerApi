using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Domain.Entities;
using UrlShortenerApi.Domain.Exceptions;

namespace UrlShortenerApi.Application.UseCases.CreateShortUrl;

public sealed class CreateShortUrlHandler
{
    private readonly IShortenedUrlRepository _repository;
    private readonly IBase62Service _base62Service;

    public CreateShortUrlHandler(IShortenedUrlRepository repository, IBase62Service base62Service)
    {
        _repository = repository;
        _base62Service = base62Service;
    }

    public async Task<CreateShortUrlResponse> HandleAsync(string url, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidUrlException(url);
        }

        var entity = new ShortenedUrl { OriginalUrl = url };
        var id = await _repository.AddAsync(entity, ct);

        return new CreateShortUrlResponse(_base62Service.Encode(id));
    }
}
