namespace UrlShortenerApi.Application.Abstractions;

public interface IShortCodeGenerator
{
    string Generate(int length = 7);
}
