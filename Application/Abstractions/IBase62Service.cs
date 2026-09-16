namespace UrlShortenerApi.Application.Abstractions;

public interface IBase62Service
{
    string Encode(int id);
    int Decode(string code);
}
