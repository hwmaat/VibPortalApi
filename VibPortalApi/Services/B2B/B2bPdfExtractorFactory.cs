using System;

namespace VibPortalApi.Services.B2B
{
    public class B2bPdfExtractorFactory : IB2bPdfExtractorFactory
    {
        private readonly IServiceProvider _provider;

        public B2bPdfExtractorFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IB2bPdfExtractor GetExtractor(string filePath, string supplierCode)
        {
            return supplierCode.ToLower() switch
            {
                "aludium" => _provider.GetRequiredService<B2bPdfExtractor_Aludium>(),
                _ => throw new NotSupportedException($"No extractor available for supplier '{supplierCode}'")
            };
        }
    }
}
