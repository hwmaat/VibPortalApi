using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core;
using VibPortalApi.Data;
using VibPortalApi.Dtos;
using VibPortalApi.Models;
using VibPortalApi.Services;


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
    public async Task<VibImportResult> ProcessPdfAsync(string filePath, string? supplierCode, string?  SupplierNr)
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
            var extractor = _pdfExtractorFactory.GetExtractor(filePath, supplierCode);
            var vib = extractor.ExtractData(filePath);

            vib.EntryDate = DateTime.Now;
            vib.UserName = "VibPortal";
            vib.Status = "Imported";

            if (!string.IsNullOrWhiteSpace(SupplierNr))
                vib.SupplierNr = SupplierNr;

                vib.Dimset = "test";

            _context.VibImport.Add(vib);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.ExtractedText = ""; // Optional: keep empty for now
            result.VibId = vib.Id;  
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error during PDF processing: {ex.Message}";
            result.VibId = 0;
        }

        return result;
    }


}
