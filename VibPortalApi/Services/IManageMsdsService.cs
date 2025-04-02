using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public interface IManageMsdsService
    {
        Task<List<VibImport>> GetAllAsync();
        Task<VibImport?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(VibImport record);
        Task<PagedResult<VibImport>> GetPagedAsync(int page, int pageSize, string sortColumn, string sortDirection, string? filter, string? status);
        (string SupplierCode, string Dimset, string Recipe) ParseFileName(string fileName);

    }
}
