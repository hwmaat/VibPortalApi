using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public interface IPdfExtractorService
    {
        VibImport ExtractData(string filePath);
    }
}
