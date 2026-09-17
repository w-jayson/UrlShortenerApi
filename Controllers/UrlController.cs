using Microsoft.AspNetCore.Mvc;
using UrlShortenerApi.Application.UseCases.CreateShortUrl;
using UrlShortenerApi.Application.UseCases.ResolveShortUrl;

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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Shorten([FromBody] CreateShortUrlRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(request, ct);

        return result.Status switch
        {
            CreateShortUrlStatus.Success => Created(
                $"{Request.Scheme}://{Request.Host}/{result.Code}",
                new
                {
                    ShortUrl = $"{Request.Scheme}://{Request.Host}/{result.Code}",
                    Code = result.Code
                }),

            CreateShortUrlStatus.InvalidUrl => BadRequest(new { message = result.ErrorMessage }),

            CreateShortUrlStatus.InvalidSlug => BadRequest(new { message = result.ErrorMessage }),

            CreateShortUrlStatus.SlugAlreadyExists => Conflict(new { message = result.ErrorMessage }),

            CreateShortUrlStatus.CollisionConflict => StatusCode(StatusCodes.Status409Conflict, new { message = result.ErrorMessage }),

            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro inesperado ao encurtar URL." })
        };
    }

    [HttpGet("{code}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectTo(string code, CancellationToken ct)
    {
        var originalUrl = await _resolveHandler.ResolveAsync(code, ct);

        if (originalUrl is null)
        {
            return NotFound(new { message = "Código de URL não encontrado." });
        }

        return Redirect(originalUrl);
    }
}
