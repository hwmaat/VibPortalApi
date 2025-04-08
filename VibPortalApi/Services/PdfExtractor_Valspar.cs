using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public class PdfExtractor_Valspar : IPdfExtractorService
    {
        public VibImport ExtractData(string filePath)
        {
            return new VibImport
            {
                Entry_Date = DateTime.Now,
                Status = "Imported"
            };
        }
    }
}
