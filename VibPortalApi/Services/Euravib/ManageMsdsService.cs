using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using VibPortalApi.Data;
using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public class ManageMsdsService : IManageMsdsService
    {
        private readonly AppDbContext _msSqlContext;

        public ManageMsdsService(AppDbContext msSqlContext)
        {
            _msSqlContext = msSqlContext;

        }
        public async Task<List<VibImport>> GetAllAsync()
        {
            return await _msSqlContext.VibImport.OrderByDescending(x => x.Entry_Date).ToListAsync();
        }

        public async Task<VibImport?> GetByIdAsync(int id)
        {
            return await _msSqlContext.VibImport.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> UpdateAsync(VibImport record)
        {
            var existing = await _msSqlContext.VibImport.FindAsync(record.Id);
            if (existing == null)
                return false;

            _msSqlContext.Entry(existing).CurrentValues.SetValues(record);
            await _msSqlContext.SaveChangesAsync();
            return true;
        }

        public async Task<VibPagedResult<VibImport>> GetPagedAsync(VibPagedRequest request)
        {
            var query = _msSqlContext.VibImport.AsNoTracking();

            // 🔍 Multi-field filter logic (case-insensitive)
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                var filter = request.Filter.Trim().ToLower();

                query = query.Where(v =>
                    v.Suppl_Nr.ToLower().Contains(filter) ||
                    v.Status.ToLower().Contains(filter) ||
                    v.H_Nr.ToLower().Contains(filter) ||
                    v.Eg_Nr.ToLower().Contains(filter));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim().ToLower();
                query = query.Where(v => v.Status.ToLower() == status);
            }

            // 📊 Dynamic sort using Linq.Dynamic.Core
            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                var orderBy = $"{request.SortColumn} {request.SortDirection}";
                query = query.OrderBy(orderBy);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new VibPagedResult<VibImport>
            {
                TotalRecords = totalCount,
                Records = data,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public (string SupplierCode, string Dimset, string Recipe) ParseFileName(string fname)
        {
            string fileName = fname;
            string supplierCode = "";
            string dimset = "";
            string recipe = "";
            bool skipMail = false;

            // Initial pattern to extract supplierCode and dimset
            var match = Regex.Match(fileName, @"^(\w\d+)[\s_]+(?:(?:((?:\d{2}[A-z]{1}|[A-z]{3})\d{4}[\s_\.]*[\w]{0,2}).*)|(.*?))\.pdf", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                supplierCode = match.Groups[1].Value;
                dimset = match.Groups[2].Value;
            }

            if (string.IsNullOrEmpty(supplierCode) || Regex.IsMatch(supplierCode, @"^s\d+", RegexOptions.IgnoreCase))
            {
                bool becker3GBMatch = Regex.IsMatch(fileName, @"United Kingdom-United Kingdom_English \(GB\)", RegexOptions.IgnoreCase);
                bool becker3NLMatch = Regex.IsMatch(fileName, @"Netherlands-Netherlands_Dutch \(NL\)|C\d+.pdf", RegexOptions.IgnoreCase);
                bool ppgMatch = Regex.IsMatch(fileName, @"EU\s+SDS\s+REACH\s*-\s*Netherlands_nl-NL.pdf", RegexOptions.IgnoreCase);
                bool brilluxMatch = Regex.IsMatch(fileName, @"s\d+_\d+un.*?\.pdf", RegexOptions.IgnoreCase);

                if (becker3GBMatch || becker3NLMatch)
                {
                    supplierCode = becker3GBMatch ? "B0199" : "00525";
                }
                else if (ppgMatch)
                {
                    supplierCode = "07425";
                }
                else if (brilluxMatch)
                {
                    supplierCode = "00940";
                }
            }

            if (string.IsNullOrEmpty(dimset))
            {
                switch (supplierCode)
                {
                    case "B0199":
                    case "00525":
                        var beckersMatch = Regex.Match(fileName, @"(^[a-zA-Z]\d{6})(-\d{5}[a-zA-Z])?");
                        recipe = beckersMatch.Success ? beckersMatch.Groups[1].Value : "";
                        break;

                    case "07425":
                        var ppgMatch = Regex.Match(fileName, @"FN_(.*?[.\s]\d+)");
                        dimset = ppgMatch.Success ? ppgMatch.Groups[1].Value.Replace(" ", ".") : "";
                        break;

                    case "00940":
                        var brilluxRecipeMatch = Regex.Match(fileName, @"s(\d+_\d+)un.*?\.pdf", RegexOptions.IgnoreCase);
                        recipe = brilluxRecipeMatch.Success ? brilluxRecipeMatch.Groups[1].Value.Replace("_", ".-.") : "";
                        break;
                }
            }

            // Special Brillux case with s123_456 naming
            if (Regex.IsMatch(supplierCode, @"^s\d+", RegexOptions.IgnoreCase))
            {
                var recipeMatch = Regex.Match(fileName, @"s(\d+)_(\d+)", RegexOptions.IgnoreCase);
                if (recipeMatch.Success)
                {
                    recipe = $"{recipeMatch.Groups[1].Value}.-.7{recipeMatch.Groups[2].Value.Substring(1)}";
                }
            }

            // Supplier-specific logic
            switch (supplierCode)
            {
                case "02910":
                    var klutheMatch = Regex.Match(fileName, @"02910[\s_]*(.*?)\.pdf", RegexOptions.IgnoreCase);
                    dimset = klutheMatch.Success ? klutheMatch.Groups[1].Value.Replace("_", " ") : "";
                    break;

                case "01270":
                    var chemetallMatch = Regex.Match(fileName, @"01270[\s_]*(.*?)\.pdf", RegexOptions.IgnoreCase);
                    dimset = chemetallMatch.Success ? chemetallMatch.Groups[1].Value.Replace("01270_", "01270 ") : "";
                    break;

                case "00940":
                    if (string.IsNullOrEmpty(recipe))
                    {
                        var dimsetMatch = Regex.Match(fileName, @"s\d+_\d+_(.*?)\.pdf", RegexOptions.IgnoreCase);
                        dimset = dimsetMatch.Success ? dimsetMatch.Groups[1].Value.Replace("_", ".") : "";
                    }
                    break;
            }

            // Skip TDS files
            skipMail = Regex.IsMatch(fileName, @"^TDS.*\.pdf", RegexOptions.IgnoreCase);

            return (supplierCode, dimset, recipe);
        }
    }
}
