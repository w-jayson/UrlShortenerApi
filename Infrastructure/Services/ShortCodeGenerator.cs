using System.Security.Cryptography;
using UrlShortenerApi.Application.Abstractions;

namespace UrlShortenerApi.Infrastructure.Services;

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Base62Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 7)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        return RandomNumberGenerator.GetString(Base62Alphabet, length);
    }
}
