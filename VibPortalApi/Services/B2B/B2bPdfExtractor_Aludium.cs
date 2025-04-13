using System.IO;
using System.Threading.Tasks;

namespace VibPortalApi.Services.B2B
{
    public class B2bPdfExtractor_Aludium : IB2bPdfExtractor
    {
        public async Task<string> ExtractTextAsync(string filePath)
        {
            // TODO: Replace with proper PDF parser (PdfPig, iText7 etc.)
            // For now, just simulate
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
