using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using VibPortalApi.Data;
using VibPortalApi.Models;
using VibPortalApi.Models.DB2Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace VibPortalApi.Services
{
    public class EuravibService : IEuravibService
    {
        private readonly DB2Context _context;


        public EuravibService(DB2Context context)
        {
            _context = context;

        }

        public async Task<List<EuravibImport>> GetAllAsync()
        {
            return await _context.EuravibImport.ToListAsync();
        }

        public async Task<EuravibImport?> GetByIdAsync(int id)
        {
            return await _context.EuravibImport.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(int id, EuravibImport record)
        {
            var existing = await _context.EuravibImport.FindAsync(id);
            if (existing == null)
                return false;

            _context.Entry(existing).CurrentValues.SetValues(record);
            await _context.SaveChangesAsync();
            return true;
        }

        //public async Task<VibPagedResult<EuravibImport>> GetPagedAsync(PagedRequest request)
        //{
        //    var query = _context.EuravibImport.AsQueryable();

        //    // Filtering
        //    if (!string.IsNullOrWhiteSpace(request.Filter))
        //    {
        //        var filter = request.Filter.Trim().ToLower();

        //        query = query.Where(v =>
        //            v.Suppl_Nr.ToLower().Contains(filter) ||
        //            v.Dimset.ToLower().Contains(filter) ||
        //            v.H_Nr.ToLower().Contains(filter) ||
        //            v.Eg_Nr.ToLower().Contains(filter));
        //    }


        //    // Sorting
        //    if (!string.IsNullOrEmpty(request.SortColumn))
        //    {
        //        var sortString = $"{request.SortColumn} {request.SortDirection}";
        //        query = query.OrderBy(sortString);
        //    }

        //    // Paging
        //    var totalRecords = await query.CountAsync();
        //    var recordsSql = query
        //        .Skip((request.Page - 1) * request.PageSize)
        //        .Take(request.PageSize);
        //    var records = await recordsSql.ToListAsync();

        //    return new VibPagedResult<EuravibImport>
        //    {
        //        Records = records,
        //        TotalRecords = totalRecords,
        //        Page = request.Page,
        //        PageSize = request.PageSize
        //    };
        //}
        public async Task<bool> DeleteAsync(string supplNr, DateTime revDate, string dimset)
        {
            var existing = await _context.EuravibImport
                .FirstOrDefaultAsync(e =>
                    e.Suppl_Nr == supplNr &&
                    e.Rev_Date == revDate &&
                    e.Dimset == dimset);

            if (existing == null) return false;

            _context.EuravibImport.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VibPagedResult<EuravibImport>> GetPagedAsync(PagedRequest request)
        {
            try
            {
                var query = _context.EuravibImport.AsQueryable();

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



        private async Task<List<EuravibImport>> GetPagedWithRowNumberAsync(PagedRequest request)
        {
            var query = _context.EuravibImport.AsQueryable();

            // Optional filtering
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                var filter = request.Filter.ToLower();
                query = query.Where(e =>
                    (e.Suppl_Nr ?? "").ToLower().Contains(filter) ||
                    (e.Dimset ?? "").ToLower().Contains(filter) ||
                    (e.H_Nr ?? "").ToLower().Contains(filter) ||
                    (e.Eg_Nr ?? "").ToLower().Contains(filter));
            }


            // Validate sort column
            var allowedColumns = new HashSet<string>
    {
        "suppl_Nr", "dimset", "rev_Date", "entry_Date", "status"
    };

            var sortColumn = allowedColumns.Contains(request.SortColumn) ? request.SortColumn : "entry_Date";
            var sortDirection = request.SortDirection?.ToLower() == "desc";

            // Sort dynamically
            query = query.OrderBy($"{sortColumn} {(sortDirection ? "descending" : "ascending")}");

            // Paging
            return await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
        }



    }
}
