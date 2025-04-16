using VibPortalApi.Dtos;

namespace VibPortalApi.Services.Euravib
{
    public interface IVibImportService
    {
        Task<VibImportResult> ProcessPdfAsync(string filePath, string? supplierCode, string? Suppl_Nr);

    }
}
