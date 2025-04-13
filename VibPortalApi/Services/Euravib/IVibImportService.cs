using Azure.Core;
using VibPortalApi.Dtos;
using VibPortalApi.Models;

namespace VibPortalApi.Services.Euravib
{
    public interface IVibImportService
    {
        Task<VibImportResult> ProcessPdfAsync(string filePath, string? supplierCode, string? Suppl_Nr);

    }
}
