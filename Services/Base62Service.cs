namespace UrlShortenerApi.Services;

public interface IBase62Service
{
    string Encode(int id);
    int Decode(string shortUrl);
}

public class Base62Service : IBase62Service
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int Base = Alphabet.Length;

    public string Encode(int id)
    {
        if (id == 0) return Alphabet[0].ToString();

        var shortUrl = string.Empty;
        while (id > 0)
        {
            shortUrl = Alphabet[id % Base] + shortUrl;
            id /= Base;
        }
        return shortUrl;
    }

    public int Decode(string shortUrl)
    {
        var id = 0;
        foreach (var c in shortUrl)
        {
            id = id * Base + Alphabet.IndexOf(c);
        }
        return id;
    }
}
