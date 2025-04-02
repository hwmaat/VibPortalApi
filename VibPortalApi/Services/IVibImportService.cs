using Azure.Core;
using VibPortalApi.Dtos;
using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public interface IVibImportService
    {
        Task<VibImportResult> ProcessPdfAsync(string filePath, string? supplierCode, string? SupplierNr);

    }
}
