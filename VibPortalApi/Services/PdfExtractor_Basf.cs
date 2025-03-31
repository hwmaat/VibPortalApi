using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public class PdfExtractor_Basf : IPdfExtractorService
    {
        public VibImport ExtractData(string filePath)
        {
            return new VibImport
            {
                EntryDate = DateTime.Now,
                Status = "Imported"
            };
        }
    }
}
