using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Domain.Exceptions;

namespace UrlShortenerApi.Infrastructure.Services;

public sealed class Base62Service : IBase62Service
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int Base = Alphabet.Length;

    public string Encode(int id)
    {
        if (id == 0) return Alphabet[0].ToString();

        var result = string.Empty;
        while (id > 0)
        {
            result = Alphabet[id % Base] + result;
            id /= Base;
        }
        return result;
    }

    public int Decode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidShortCodeException(code ?? string.Empty);

        var id = 0;
        foreach (var c in code)
        {
            var index = Alphabet.IndexOf(c);
            if (index < 0)
                throw new InvalidShortCodeException(code);

            id = id * Base + index;
        }
        return id;
    }
}
