using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using VibPortalApi.Data;
using VibPortalApi.Dtos;
using VibPortalApi.Models;
using VibPortalApi.Services;

public interface IVibImportService
{
    Task<VibImportResult> ProcessPdfAsync(string filePath, int? supplierId, string? product, string supplierCode);
    Task<List<VibImport>> GetAllAsync();
    Task<VibImport?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(VibImport record);
    Task<PagedResult<VibImport>> GetPagedAsync(int page, int pageSize, string sortColumn, string sortDirection, string? filter, string? status);
}

public class VibImportService : IVibImportService
{
    private readonly AppDbContext _context;
    private readonly IPdfExtractorFactory _pdfExtractorFactory;

    public VibImportService(AppDbContext context, IPdfExtractorFactory pdfExtractorFactory)
    {
        _context = context;
        _pdfExtractorFactory = pdfExtractorFactory;
    }

    // === PDF Upload & Processing ===
    public async Task<VibImportResult> ProcessPdfAsync(string filePath, int? supplierId, string? product, string supplierCode)
    {
        var result = new VibImportResult();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            result.Success = false;
            result.ErrorMessage = "PDF file not found.";
            return result;
        }

        try
        {
            var extractor = _pdfExtractorFactory.GetExtractor(supplierCode);
            var vib = extractor.ExtractData(filePath);

            vib.EntryDate = DateTime.Now;
            vib.UserName = "System";
            vib.Status = "Imported";

            if (supplierId.HasValue)
                vib.SupplierNr = supplierId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(product))
                vib.Dimset = product;

            _context.VibImport.Add(vib);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.ExtractedText = ""; // Optional: keep empty for now
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error during PDF processing: {ex.Message}";
        }

        return result;
    }

    // === Manage MSDS ===

    public async Task<List<VibImport>> GetAllAsync()
    {
        return await _context.VibImport.OrderByDescending(x => x.EntryDate).ToListAsync();
    }

    public async Task<VibImport?> GetByIdAsync(int id)
    {
        return await _context.VibImport.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> UpdateAsync(VibImport record)
    {
        var existing = await _context.VibImport.FindAsync(record.Id);
        if (existing == null)
            return false;

        _context.Entry(existing).CurrentValues.SetValues(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<VibImport>> GetPagedAsync(int page, int pageSize, string sortColumn, string sortDirection, string? filter, string? status)
    {
        var query = _context.VibImport.AsNoTracking();

        // 🔍 Apply filtering
        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim().ToLower();

            query = query.Where(v =>
                v.SupplierNr.ToLower().Contains(filter) ||
                v.Status.ToLower().Contains(filter) ||
                v.H_Number.ToLower().Contains(filter) ||
                v.EgNumber.ToLower().Contains(filter));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(v => v.Status.ToLower() == status.ToLower());
        }
        // 🧠 Apply dynamic sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            var orderBy = $"{sortColumn} {sortDirection}";
            query = query.OrderBy(orderBy); // using System.Linq.Dynamic.Core
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<VibImport>
        {
            TotalCount = totalCount,
            Data = data
        };
    }
}
