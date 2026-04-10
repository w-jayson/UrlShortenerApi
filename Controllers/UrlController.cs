using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;
using UrlShortenerApi.Services;

namespace UrlShortenerApi.Controllers;

[ApiController]
[Route("")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IBase62Service _base62Service;

    public UrlController(AppDbContext context, IBase62Service base62Service)
    {
        _context = context;
        _base62Service = base62Service;

    }

    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return BadRequest("URL inválida.");


        var shortenedUrl = new ShortenedUrl
        {
            OriginalUrl = url,
            ShortCode = string.Empty
        };

        _context.ShortenedUrls.Add(shortenedUrl);
        await _context.SaveChangesAsync();

        shortenedUrl.ShortCode = _base62Service.Encode(shortenedUrl.Id);
        await _context.SaveChangesAsync();

        return Ok(new { ShortUrl = $"{Request.Scheme}://{Request.Host}/{shortenedUrl.ShortCode}" });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectTo(string code)
    {
        var id = _base62Service.Decode(code);
        var entry = await _context.ShortenedUrls.FindAsync(id);

        if (entry == null)
            return NotFound("Código de URL não encontrado.");

        return Redirect(entry.OriginalUrl);
    }

}