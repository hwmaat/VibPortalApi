using Microsoft.AspNetCore.Mvc;
using VibPortalApi.Services.B2B;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class B2BController : ControllerBase
    {
        private readonly IB2BImportOc _b2bImportOc;

        public B2BController(IB2BImportOc b2bImportOc)
        {
            _b2bImportOc = b2bImportOc;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<B2BProcessResult>> UploadB2BOrderConfirmation(
            IFormFile file,
            [FromForm] string supplierCode)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new B2BProcessResult
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "No file uploaded"
                });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _b2bImportOc.B2BProcessOcAsync(stream, string.Empty, file.FileName, supplierCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new B2BProcessResult
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
