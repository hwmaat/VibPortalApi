using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VibPortalApi.Data;
using VibPortalApi.Models.B2B;
using VibPortalApi.Models.Settings;
using VibPortalApi.Services.B2B.Azure;

namespace VibPortalApi.Services.B2B
{
    public class B2BImportOc : IB2BImportOc
    {
        private readonly AppDbContext _db;
        private readonly AppSettings _settings;
        private readonly DocumentAnalysisClient _formRecognizer;
        private readonly ILogger<B2BImportOc> _logger;
        private readonly IB2BFormRecognizerFactory _b2bFormRecognizerFactory;
        public B2BImportOc(
            AppDbContext db,
            IOptions<AppSettings> settings,
            DocumentAnalysisClient formRecognizer,
            ILogger<B2BImportOc> logger,
            IB2BFormRecognizerFactory b2bFormRecognizerFactory)
        {
            _db = db;
            _settings = settings.Value;
            _formRecognizer = formRecognizer;
            _logger = logger;
            _b2bFormRecognizerFactory = b2bFormRecognizerFactory;


        }

        public async Task<B2BProcessResult> B2BProcessOcAsync(Stream attachmentContent, string gmailId, string attachmentName, string supplierCode)
        {
            var result = new B2BProcessResult { GmailId = gmailId };

            try
            {
                var b2bPath = _settings.B2BPath;
                if (string.IsNullOrEmpty(b2bPath))
                    throw new Exception("B2B path is not configured in AppSettings");

                Directory.CreateDirectory(b2bPath);
                var fullPath = Path.Combine(b2bPath, attachmentName);

                // Save the attachment file to disk
                using (var fileStream = File.Create(fullPath))
                {
                    await attachmentContent.CopyToAsync(fileStream);
                }

                // Analyze the PDF using Azure Form Recognizer
                attachmentContent.Position = 0; // rewind stream if already used
                var analyzeOp = await _formRecognizer.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", attachmentContent);
                var mapper = _b2bFormRecognizerFactory.GetMapper(supplierCode);
                var parsed = mapper.Map(analyzeOp.Value);

                // Insert or update header record
                var existing = await _db.B2BSupplierOcs
                    .FirstOrDefaultAsync(x => x.AttachtmentName == attachmentName);

                int ocId;

                if (existing != null)
                {
                    existing.GmailId = string.IsNullOrEmpty(gmailId) ? null : gmailId;
                    existing.Status = "seen";
                    existing.Supplier_Nr = parsed.Supplier_Nr;
                    existing.OrderDate = parsed.OrderDate;
                    existing.OrderNr = parsed.OrderNr;
                    existing.CustomerNr = parsed.CustomerNr;
                    existing.EuramaxPo_Nr = parsed.EuramaxPo_Nr;

                    ocId = existing.Oc_Id;
                }
                else
                {
                    var newOc = new B2BSupplierOc
                    {
                        GmailId = string.IsNullOrEmpty(gmailId) ? null : gmailId,
                        AttachtmentName = attachmentName,
                        Status = "seen",
                        Supplier_Nr = parsed.Supplier_Nr,
                        OrderDate = parsed.OrderDate,
                        OrderNr = parsed.OrderNr,
                        CustomerNr = parsed.CustomerNr,
                        EuramaxPo_Nr = parsed.EuramaxPo_Nr
                    };

                    _db.B2BSupplierOcs.Add(newOc);
                    await _db.SaveChangesAsync(); // to get Oc_Id
                    ocId = newOc.Oc_Id;
                }

                // Delete previous lines for this OC (if reprocessing)
                await _db.B2BSupplierOcLines
                    .Where(x => x.Oc_Id == ocId)
                    .ExecuteDeleteAsync();

                // Insert new line items
                foreach (var line in parsed.Lines)
                {
                    _db.B2BSupplierOcLines.Add(new B2BSupplierOcLine
                    {
                        Oc_Id = ocId,
                        Line = line.Line,
                        Dimset = line.Dimset,
                        SupplierPartNr = line.SupplierPartNr,
                        Quantity_Kg = line.Quantity_Kg,
                        Price_t = line.Price_t,
                        Currency = line.Currency,
                        Specification = line.Specification
                    });
                }

                await _db.SaveChangesAsync();

                result.Success = true;
                result.Status = "processed";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process B2B OC attachment '{Attachment}'", attachmentName);
                result.Success = false;
                result.Status = "failed";
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
    }
}
