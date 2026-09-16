using Microsoft.AspNetCore.Mvc;
using UrlShortenerApi.Application.UseCases.CreateShortUrl;
using UrlShortenerApi.Application.UseCases.ResolveShortUrl;
using UrlShortenerApi.Domain.Exceptions;

namespace UrlShortenerApi.Controllers;

[ApiController]
[Route("")]
public class UrlController : ControllerBase
{
    private readonly CreateShortUrlHandler _createHandler;
    private readonly ResolveShortUrlHandler _resolveHandler;

    public UrlController(CreateShortUrlHandler createHandler, ResolveShortUrlHandler resolveHandler)
    {
        _createHandler = createHandler;
        _resolveHandler = resolveHandler;
    }

    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] string url, CancellationToken ct)
    {
        try
        {
            var response = await _createHandler.HandleAsync(url, ct);
            return Ok(new { ShortUrl = $"{Request.Scheme}://{Request.Host}/{response.ShortCode}" });
        }
        catch (InvalidUrlException)
        {
            return BadRequest("URL inválida.");
        }
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectTo(string code, CancellationToken ct)
    {
        try
        {
            var originalUrl = await _resolveHandler.ResolveAsync(code, ct);
            if (originalUrl is null)
                return NotFound("Código de URL não encontrado.");

            return Redirect(originalUrl);
        }
        catch (InvalidShortCodeException)
        {
            return BadRequest("Código de URL inválido.");
        }
    }
}
