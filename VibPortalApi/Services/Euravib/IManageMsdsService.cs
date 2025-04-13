using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public interface IManageMsdsService
    {
        Task<List<VibImport>> GetAllAsync();
        Task<VibImport?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(VibImport record);
        Task<VibPagedResult<VibImport>> GetPagedAsync(VibPagedRequest request);
        (string SupplierCode, string Dimset, string Recipe) ParseFileName(string fileName);

    }
}
