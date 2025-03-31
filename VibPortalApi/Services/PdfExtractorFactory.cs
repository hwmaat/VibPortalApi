namespace VibPortalApi.Services
{
    public class PdfExtractorFactory : IPdfExtractorFactory
    {
        private readonly IServiceProvider _provider;

        public PdfExtractorFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IPdfExtractorService GetExtractor(string supplierCode)
        {
            return supplierCode.ToLowerInvariant() switch
            {
                "akzo" => _provider.GetRequiredService<PdfExtractor_Akzo>(),
                "basf" => _provider.GetRequiredService<PdfExtractor_Basf>(),
                "beckers" => _provider.GetRequiredService<PdfExtractor_Beckers>(),
                "brillux" => _provider.GetRequiredService<PdfExtractor_Brillux>(),
                "kluthe" => _provider.GetRequiredService<PdfExtractor_Kluthe>(),
                "monopol" => _provider.GetRequiredService<PdfExtractor_Monopol>(),
                "ppg" => _provider.GetRequiredService<PdfExtractor_Ppg>(),
                "valspar" => _provider.GetRequiredService<PdfExtractor_Valspar>(),
                _ => throw new NotSupportedException($"No PDF extractor registered for supplier '{supplierCode}'")
            };
        }
    }
}
