// Entidade EF Core que representa uma URL encurtada persistida no banco SQLite.
namespace UrlShortenerApi.Models;

public class ShortenedUrl
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public long ClickCount { get; set; }
    // Armazena datas sempre em UTC para evitar divergências de fuso horário.
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastAccessedAtUtc { get; set; }
}
