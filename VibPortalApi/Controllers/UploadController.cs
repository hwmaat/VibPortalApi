using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
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

            if (request.SupplierCode.IsNullOrEmpty())
                return BadRequest("SupplierCode is required.");

            var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_" + request.File.FileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await request.File.CopyToAsync(stream);
            }

            var Suppl_Nr = ParseFileName(request.File.FileName).SupplierCode;
            var supplierName = ResolveSupplierCode(Suppl_Nr);
            
            var result = await _vibImportService.ProcessPdfAsync(filePath, supplierName, Suppl_Nr);

            return result.Success ? Ok(result) : BadRequest(result.ErrorMessage);
        }

        private string ResolveSupplierCode(string supplierCode)
        {
            return supplierCode switch
            {
                "01270" => "gardobond",
                "1001" => "akzo",
                "1002" => "basf",
                "00525" => "beckers",
                "1004" => "brillux",
                "1005" => "kluthe",
                "1006" => "monopol",
                "1007" => "ppg",
                "1008" => "valspar",
                _ => throw new NotSupportedException($"No extractor registered for supplier ID '{supplierCode}'")
            };
        }
        private (string SupplierCode, string Dimset, string Recipe) ParseFileName(string fname)
        {
            string fileName = fname;
            string supplierCode = "";
            string dimset = "";
            string recipe = "";
            bool skipMail = false;

            // Initial pattern to extract supplierCode and dimset
            var match = Regex.Match(fileName, @"^(\w\d+)[\s_]+(?:(?:((?:\d{2}[A-z]{1}|[A-z]{3})\d{4}[\s_\.]*[\w]{0,2}).*)|(.*?))\.pdf", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                supplierCode = match.Groups[1].Value;
                dimset = match.Groups[2].Value;
            }

            if (string.IsNullOrEmpty(supplierCode) || Regex.IsMatch(supplierCode, @"^s\d+", RegexOptions.IgnoreCase))
            {
                bool becker3GBMatch = Regex.IsMatch(fileName, @"United Kingdom-United Kingdom_English \(GB\)", RegexOptions.IgnoreCase);
                bool becker3NLMatch = Regex.IsMatch(fileName, @"Netherlands-Netherlands_Dutch \(NL\)|C\d+.pdf", RegexOptions.IgnoreCase);
                bool ppgMatch = Regex.IsMatch(fileName, @"EU\s+SDS\s+REACH\s*-\s*Netherlands_nl-NL.pdf", RegexOptions.IgnoreCase);
                bool brilluxMatch = Regex.IsMatch(fileName, @"s\d+_\d+un.*?\.pdf", RegexOptions.IgnoreCase);

                if (becker3GBMatch || becker3NLMatch)
                {
                    supplierCode = becker3GBMatch ? "B0199" : "00525";
                }
                else if (ppgMatch)
                {
                    supplierCode = "07425";
                }
                else if (brilluxMatch)
                {
                    supplierCode = "00940";
                }
            }

            if (string.IsNullOrEmpty(dimset))
            {
                switch (supplierCode)
                {
                    case "B0199":
                    case "00525":
                        var beckersMatch = Regex.Match(fileName, @"(^[a-zA-Z]\d{6})(-\d{5}[a-zA-Z])?");
                        recipe = beckersMatch.Success ? beckersMatch.Groups[1].Value : "";
                        break;

                    case "07425":
                        var ppgMatch = Regex.Match(fileName, @"FN_(.*?[.\s]\d+)");
                        dimset = ppgMatch.Success ? ppgMatch.Groups[1].Value.Replace(" ", ".") : "";
                        break;

                    case "00940":
                        var brilluxRecipeMatch = Regex.Match(fileName, @"s(\d+_\d+)un.*?\.pdf", RegexOptions.IgnoreCase);
                        recipe = brilluxRecipeMatch.Success ? brilluxRecipeMatch.Groups[1].Value.Replace("_", ".-.") : "";
                        break;
                }
            }

            // Special Brillux case with s123_456 naming
            if (Regex.IsMatch(supplierCode, @"^s\d+", RegexOptions.IgnoreCase))
            {
                var recipeMatch = Regex.Match(fileName, @"s(\d+)_(\d+)", RegexOptions.IgnoreCase);
                if (recipeMatch.Success)
                {
                    recipe = $"{recipeMatch.Groups[1].Value}.-.7{recipeMatch.Groups[2].Value.Substring(1)}";
                }
            }

            // Supplier-specific logic
            switch (supplierCode)
            {
                case "02910":
                    var klutheMatch = Regex.Match(fileName, @"02910[\s_]*(.*?)\.pdf", RegexOptions.IgnoreCase);
                    dimset = klutheMatch.Success ? klutheMatch.Groups[1].Value.Replace("_", " ") : "";
                    break;

                case "01270":
                    var chemetallMatch = Regex.Match(fileName, @"01270[\s_]*(.*?)\.pdf", RegexOptions.IgnoreCase);
                    dimset = chemetallMatch.Success ? chemetallMatch.Groups[1].Value.Replace("01270_", "01270 ") : "";
                    break;

                case "00940":
                    if (string.IsNullOrEmpty(recipe))
                    {
                        var dimsetMatch = Regex.Match(fileName, @"s\d+_\d+_(.*?)\.pdf", RegexOptions.IgnoreCase);
                        dimset = dimsetMatch.Success ? dimsetMatch.Groups[1].Value.Replace("_", ".") : "";
                    }
                    break;
            }

            // Skip TDS files
            skipMail = Regex.IsMatch(fileName, @"^TDS.*\.pdf", RegexOptions.IgnoreCase);

            return (supplierCode, dimset, recipe);
        }
    
}
}