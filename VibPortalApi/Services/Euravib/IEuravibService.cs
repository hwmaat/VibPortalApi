using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VibPortalApi.Models.DB2Models;
using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public interface IEuravibService
    {
        Task<List<EuravibImport>> GetAllAsync();
        Task<EuravibImport?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, EuravibImport model);
        Task<bool> DeleteAsync(string supplNr, DateTime revDate, string dimset);
        Task<VibPagedResult<EuravibImport>> GetPagedAsync(VibPagedRequest request);
    }
}
