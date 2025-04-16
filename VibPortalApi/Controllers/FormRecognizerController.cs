using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VibPortalApi.Services.B2B.Azure;

namespace VibPortalApi.Controllers.Dev
{
    [ApiController]
    [Route("api/dev/formrecognizer")]
    public class FormRecognizerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FormRecognizerController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AnalyzePdf([FromForm] PdfUploadRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("PDF file is required.");

            var endpoint = _config["AzureFormRecognizer:Endpoint"];
            var apiKey = _config["AzureFormRecognizer:ApiKey"];

            var recognizer = new AzureFormRecognizerService(endpoint, apiKey);

            using var stream = file.OpenReadStream();
            var result = await recognizer.AnalyzePdfAsync(stream);

            return Ok(result);
        }
    }
    public class PdfUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
