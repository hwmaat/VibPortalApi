using System.Threading.Tasks;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B.Extractors
{
    public interface IB2bPdfExtractor
    {
        Task<B2BParsedOcData> ExtractTextAsync(string filePath);
    }
}