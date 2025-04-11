using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VibPortalApi.Models;
using VibPortalApi.Models.DB2Models;

namespace VibPortalApi.Services
{
    public interface IEuravibService
    {
        Task<List<EuravibImport>> GetAllAsync();
        Task<EuravibImport?> GetByIdAsync(string supplNr, DateTime revDate, string dimset);
        Task<bool> UpdateAsync(EuravibImport record);
        Task<bool> DeleteAsync(string supplNr, DateTime revDate, string dimset);
        Task<VibPagedResult<EuravibImport>> GetPagedAsync(PagedRequest request);
    }
}
