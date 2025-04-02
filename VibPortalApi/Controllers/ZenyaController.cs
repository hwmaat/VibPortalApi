using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Services;

namespace VibPortalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZenyaController : ControllerBase
    {
        private readonly IZenyaService _zenyaService;

        public ZenyaController(IZenyaService zenyaService)
        {
            _zenyaService = zenyaService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _zenyaService.GetSearchSuggestionAsync(query);
            if (string.IsNullOrEmpty(result))
                return NotFound("Geen suggestie gevonden");

            return Ok(result);
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var token = await _zenyaService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return StatusCode(500, "Kan geen token ophalen");

            return Ok(new { token });
        }
        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            var version = await _zenyaService.GetVersionAsync();
            return Ok(version);
        }

        [HttpPost("set-filter")]
        public async Task<IActionResult> SetFilter([FromQuery] string title, [FromQuery] int folderId)
        {
            var result = await _zenyaService.SetDocumentFilterAsync(title, folderId);
            return Ok(result);
        }

        [HttpGet("documents/by-filter/{filterId}")]
        public async Task<IActionResult> GetDocumentsByFilter(string filterId)
        {
            var docs = await _zenyaService.GetDocumentsByFilterAsync(filterId);
            return docs != null ? Ok(docs) : NotFound();
        }

        [HttpGet("search-document")]
        public async Task<IActionResult> SearchDocument([FromQuery] string title, [FromQuery] int folderId)
        {
            var docs = await _zenyaService.SearchDocumentAsync(title, folderId);
            if (docs == null || !docs.Any()) return NotFound("No documents found.");
            return Ok(docs);
        }

    }
}
