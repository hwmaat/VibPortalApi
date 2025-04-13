using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public interface IPdfExtractorService
    {
        VibImport ExtractData(string filePath);
    }
}
