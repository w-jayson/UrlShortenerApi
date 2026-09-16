namespace UrlShortenerApi.Domain.Exceptions;

public sealed class InvalidUrlException : Exception
{
    public InvalidUrlException(string url)
        : base($"URL inválida: '{url}'.")
    {
    }
}
