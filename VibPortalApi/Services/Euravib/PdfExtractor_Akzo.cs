using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public class PdfExtractor_Akzo : IPdfExtractorService
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
