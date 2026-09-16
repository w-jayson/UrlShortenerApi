namespace UrlShortenerApi.Domain.Exceptions;

public sealed class InvalidShortCodeException : Exception
{
    public InvalidShortCodeException(string code)
        : base($"Código de URL inválido: '{code}'.")
    {
    }
}
