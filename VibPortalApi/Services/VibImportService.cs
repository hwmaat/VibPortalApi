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
    Task<(List<VibImport> Records, int TotalCount)> GetPagedAsync(int page, int pageSize, string sortColumn, string sortDirection, string? statusFilter, string? searchQuery);
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

    public async Task<(List<VibImport> Records, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string sortColumn,
        string sortDirection,
        string? statusFilter,
        string? searchQuery)
    {
        var query = _context.VibImport.AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(x => x.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(x =>
                x.SupplierNr.Contains(searchQuery) ||
                x.Dimset.Contains(searchQuery) ||
                x.Cas_Number.Contains(searchQuery) ||
                x.Cas_Percentages.Contains(searchQuery) ||
                x.FlashPoint.Contains(searchQuery));
        }

        var totalCount = await query.CountAsync();

        // Sorting with fallback
        var orderString = $"{sortColumn} {sortDirection}";
        if (string.IsNullOrWhiteSpace(sortColumn) || !typeof(VibImport).GetProperties().Any(p => p.Name.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
        {
            orderString = "EntryDate desc";
        }

        var items = await query
            .OrderBy(orderString)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
