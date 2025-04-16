using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Models.Gmail;
using VibPortalApi.Services.Gmail;

namespace VibPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class B2BGmailController : ControllerBase
{
    private readonly IB2BGmailService _gmailService;

    public B2BGmailController(IB2BGmailService gmailService)
    {
        _gmailService = gmailService;
    }

    [HttpGet("b2bmessages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than zero.");

        var result = await _gmailService.GetB2BMessagesPagedAsync(page, pageSize, search, status);
        return Ok(result);
    }
    [HttpPost("process-b2bemail")]
    public async Task<IActionResult> ProcessB2BEmail([FromBody] ProcessEmailRequest request)
    {
        var result = await _gmailService.ProcessB2BEmailAsync(request.MessageId, request.SupplierCode, request.AttachmentName);
        return Ok(result); // Always HTTP 200; frontend handles result.Success
    }
}
