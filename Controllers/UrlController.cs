using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Controllers;

[ApiController]
[Route("")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;

    public UrlController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return BadRequest("URL inválida.");

        var code = GenerateRandomCode();

        while (await _context.ShortenedUrls.AnyAsync(x => x.ShortCode == code))
        {
            code = GenerateRandomCode();
        }

        var shortenedUrl = new ShortenedUrl
        {
            OriginalUrl = url,
            ShortCode = code
        };

        _context.ShortenedUrls.Add(shortenedUrl);
        await _context.SaveChangesAsync();

        return Ok(new { ShortUrl = $"{Request.Scheme}://{Request.Host}/{code}" });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectTo(string code)
    {
        var entry = await _context.ShortenedUrls
            .FirstOrDefaultAsync(x => x.ShortCode == code);

        if (entry == null) return NotFound();

        return Redirect(entry.OriginalUrl);
    }

    private string GenerateRandomCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}