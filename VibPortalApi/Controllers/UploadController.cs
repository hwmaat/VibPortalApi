using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VibPortalApi.Dtos;
using VibPortalApi.Services;

namespace VibPortalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IVibImportService _vibImportService;
        private readonly IPdfExtractorFactory _pdfExtractorFactory;

        public UploadController(
            IVibImportService vibImportService,
            IPdfExtractorFactory pdfExtractorFactory)
        {
            _vibImportService = vibImportService;
            _pdfExtractorFactory = pdfExtractorFactory;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf([FromForm] UploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No PDF uploaded.");

            if (!request.SupplierId.HasValue)
                return BadRequest("SupplierId is required.");

            var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_" + request.File.FileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await request.File.CopyToAsync(stream);
            }

            var supplierCode = ResolveSupplierCode(request.SupplierId.Value);

            var result = await _vibImportService.ProcessPdfAsync(filePath, request.SupplierId, request.Product, supplierCode);

            return result.Success ? Ok(result) : BadRequest(result.ErrorMessage);
        }

        // ✅ This goes here
        private string ResolveSupplierCode(int supplierId)
        {
            return supplierId switch
            {
                1001 => "akzo",
                1002 => "basf",
                1003 => "beckers",
                1004 => "brillux",
                1005 => "kluthe",
                1006 => "monopol",
                1007 => "ppg",
                1008 => "valspar",
                _ => throw new NotSupportedException($"No extractor registered for supplier ID '{supplierId}'")
            };
        }
    }
}