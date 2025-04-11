using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VibPortalApi.Data;
using VibPortalApi.Models;
using VibPortalApi.Models.DB2Models;

namespace VibPortalApi.Services
{
    public class EuravibService : IEuravibService
    {
        private readonly DB2Context _context;

        private readonly IMapper _mapper;
        public EuravibService(DB2Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EuravibImport>> GetAllAsync()
        {
            return await _context.EuravibImport.ToListAsync();
        }

        public async Task<EuravibImport?> GetByIdAsync(string supplNr, DateTime revDate, string dimset)
        {
            return await _context.EuravibImport
                .FirstOrDefaultAsync(x =>
                    x.Suppl_Nr == supplNr &&
                    x.Rev_Date == revDate &&
                    x.Dimset == dimset);
        }

        public async Task<bool> UpdateAsync(EuravibImport record)
        {
            var existing = await _context.EuravibImport
                .FirstOrDefaultAsync(x =>
                    x.Suppl_Nr == record.Suppl_Nr &&
                    x.Rev_Date == record.Rev_Date &&
                    x.Dimset == record.Dimset);

            if (existing == null) return false;

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
                var totalCount = await _context.EuravibImport.CountAsync(); // Optional: apply same filter if you want filtered count

                var raw = await GetPagedWithRowNumberAsync(request); // Returns List<EuravibImportDto>

                var data = _mapper.Map<List<EuravibImport>>(raw); // Map to real model

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


        private async Task<List<EuravibImportDto>> GetPagedWithRowNumberAsync(PagedRequest request)
        {
            var allowedColumns = new HashSet<string>
    {
        "suppl_Nr", "dimset", "rev_Date", "entry_Date", "status"
        // Add more allowed columns if needed
    };

            var sortColumn = allowedColumns.Contains(request.SortColumn) ? request.SortColumn : "entry_Date";
            var sortDirection = request.SortDirection.ToLower() == "desc" ? "DESC" : "ASC";

            var startRow = (request.Page - 1) * request.PageSize + 1;
            var endRow = request.Page * request.PageSize;

            var filterClause = "";
            var filterParams = new List<object>();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                filterClause = @"
            AND (
                LOWER(Suppl_Nr) LIKE ? OR
                LOWER(Dimset) LIKE ? OR
                LOWER(Status) LIKE ? OR
                LOWER(H_Nr) LIKE ? OR
                LOWER(Eg_Nr) LIKE ?
            )";
                var filter = $"%{request.Filter.ToLower()}%";
                filterParams.AddRange(Enumerable.Repeat<object>(filter, 5));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filterClause += " AND LOWER(Status) = ?";
                filterParams.Add(request.Status.ToLower());
            }

            var sql = $@"
    SELECT 
    RowNum,
    Suppl_Nr,
    Rev_Date,
    Dimset,
    Entry_Date,
    Cas_Nr,
    Cas_Perc,
    H_Nr,
    H_Cat,
    Adr_Un_Nr,
    Adr_Cargo_Name,
    Adr_TransportHazard_Class,
    Adr_Packing_Group,
    Adr_Environment_Hazards,
    Adr_ExtraInfo,
    Imdg_Un_Nr,
    Imdg_Cargo_Name,
    Imdg_TransportHazard_Class,
    Imdg_Packing_Group,
    Imdg_Environment_Hazards,
    Imdg_ExtraInfo,
    ExtraInfo_TunnelCode,
    FlashPoint,
    Ems_Fire,
    Ems_Spillage,
    ""USER"",
    Eg_Nr
 FROM (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY {sortColumn} {sortDirection}) AS RowNum,
    e.Suppl_Nr,
    e.Rev_Date,
    e.Dimset,
    e.Entry_Date,
    e.Cas_Nr,
    e.Cas_Perc,
    e.H_Nr,
    e.H_Cat,
    e.Adr_Un_Nr,
    e.Adr_Cargo_Name,
    e.Adr_TransportHazard_Class,
    e.Adr_Packing_Group,
    e.Adr_Environment_Hazards,
    e.Adr_ExtraInfo,
    e.Imdg_Un_Nr,
    e.Imdg_Cargo_Name,
    e.Imdg_TransportHazard_Class,
    e.Imdg_Packing_Group,
    e.Imdg_Environment_Hazards,
    e.Imdg_ExtraInfo,
    e.ExtraInfo_TunnelCode,
    e.FlashPoint,
    e.Ems_Fire,
    e.Ems_Spillage,
    e.""USER"",
    e.Eg_Nr
        FROM Euravib.Euravib_Import_Test AS e
        WHERE 1=1
        {filterClause}
    ) AS Paged
    WHERE RowNum BETWEEN ? AND ?
";

            // Add paging params last
            filterParams.Add(startRow);
            filterParams.Add(endRow);

            return await _context.EuravibImportView
                .FromSqlRaw(sql, filterParams.ToArray())
                .ToListAsync();
        }


    }
}
