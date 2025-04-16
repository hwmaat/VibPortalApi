using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using VibPortalApi.Data;
using VibPortalApi.Models.DB2Models;
using VibPortalApi.Models.Vib;

/*
 Manages the table Euravib_Import_Test, later Euravib_Import
 
 */
namespace VibPortalApi.Services.Euravib
{
    public class EuravibService : IEuravibService
    {
        private readonly DB2Context _db2Context;


        public EuravibService(DB2Context db2Context)
        {
            _db2Context = db2Context;

        }

        public async Task<List<EuravibImport>> GetAllAsync()
        {
            return await _db2Context.EuravibImport.ToListAsync();
        }

        public async Task<EuravibImport?> GetByIdAsync(int id)
        {
            return await _db2Context.EuravibImport.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(int id, EuravibImport record)
        {
            var existing = await _db2Context.EuravibImport.FindAsync(id);
            if (existing == null)
                return false;

            _db2Context.Entry(existing).CurrentValues.SetValues(record);
            await _db2Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string supplNr, DateTime revDate, string dimset)
        {
            var existing = await _db2Context.EuravibImport
                .FirstOrDefaultAsync(e =>
                    e.Suppl_Nr == supplNr &&
                    e.Rev_Date == revDate &&
                    e.Dimset == dimset);

            if (existing == null) return false;

            _db2Context.EuravibImport.Remove(existing);
            await _db2Context.SaveChangesAsync();
            return true;
        }

        public async Task<VibPagedResult<EuravibImport>> GetPagedAsync(VibPagedRequest request)
        {
            try
            {
                var query = _db2Context.EuravibImport.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    var filter = request.Filter.ToLower();
                    query = query.Where(e =>
                        (e.Suppl_Nr ?? "").ToLower().Contains(filter) ||
                        (e.Dimset ?? "").ToLower().Contains(filter) ||
                        (e.H_Nr ?? "").ToLower().Contains(filter) ||
                        (e.Eg_Nr ?? "").ToLower().Contains(filter));
                }


                var totalCount = await query.CountAsync();

                // Validate sort column
                var allowedColumns = new HashSet<string>
        {
            "suppl_Nr", "dimset", "rev_Date", "entry_Date", "status"
        };

                var sortColumn = allowedColumns.Contains(request.SortColumn) ? request.SortColumn : "entry_Date";
                var sortDirection = request.SortDirection?.ToLower() == "desc";

                query = query.OrderBy($"{sortColumn} {(sortDirection ? "descending" : "ascending")}");

                var dataSql = query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);


                var data = await dataSql.ToListAsync();
                return new VibPagedResult<EuravibImport>
                {
                    Records = data,
                    TotalRecords = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Status = "success"
                };
            }
            catch (Exception ex)
            {
                return new VibPagedResult<EuravibImport>
                {
                    Records = [],
                    TotalRecords = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Status = "failed",
                    Message = $"Failed to load data: {ex.Message}"
                };
            }
        }





    }
}
