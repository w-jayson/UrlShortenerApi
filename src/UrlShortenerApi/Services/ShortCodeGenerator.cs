// Gera códigos curtos aleatórios em Base62 com 7 caracteres para identificar as URLs.
using System.Security.Cryptography;

namespace UrlShortenerApi.Services;

public class ShortCodeGenerator
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int CodeLength = 7;

    // Método virtual para permitir sobrescrita simples nos testes unitários (simulação de colisão).
    public virtual string Generate() =>
        string.Create(CodeLength, Alphabet, static (span, alphabet) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                // RandomNumberGenerator é thread-safe e criptograficamente seguro, evitando colisões previsíveis.
                span[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
}
