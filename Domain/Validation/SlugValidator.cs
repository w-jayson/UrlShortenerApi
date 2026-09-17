using System.Text.RegularExpressions;

namespace UrlShortenerApi.Domain.Validation;

public static partial class SlugValidator
{
    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "swagger",
        "api",
        "health",
        "metrics",
        "favicon.ico",
        "robots.txt"
    };

    [GeneratedRegex("^[a-zA-Z0-9_-]{3,50}$")]
    private static partial Regex SlugRegex();

    public static bool IsValid(string? slug, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            errorMessage = "O slug customizado não pode ser vazio.";
            return false;
        }

        if (slug.Length is < 3 or > 50)
        {
            errorMessage = "O slug customizado deve conter entre 3 e 50 caracteres.";
            return false;
        }

        if (ReservedSlugs.Contains(slug))
        {
            errorMessage = $"O slug '{slug}' é reservado pelo sistema.";
            return false;
        }

        if (!SlugRegex().IsMatch(slug))
        {
            errorMessage = "O slug customizado deve conter apenas caracteres alfanuméricos, hífen (-) e underscore (_).";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static bool IsValid(string? slug) => IsValid(slug, out _);
}
