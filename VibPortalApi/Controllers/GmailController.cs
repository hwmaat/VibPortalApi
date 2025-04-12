using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Services.Gmail;

namespace VibPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GmailController : ControllerBase
{
    private readonly IGmailService _gmailService;

    public GmailController(IGmailService gmailService)
    {
        _gmailService = gmailService;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than zero.");

        var result = await _gmailService.GetMessagesPagedAsync(page, pageSize, search, status);
        return Ok(result);
    }
}
