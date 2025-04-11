using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public interface IManageMsdsService
    {
        Task<List<VibImport>> GetAllAsync();
        Task<VibImport?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(VibImport record);
        Task<VibPagedResult<VibImport>> GetPagedAsync(PagedRequest request);
        (string SupplierCode, string Dimset, string Recipe) ParseFileName(string fileName);

    }
}
