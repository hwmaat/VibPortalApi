using System.Threading.Tasks;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B
{
    public interface IB2BImportOc
    {
        Task<B2BProcessResult> B2BProcessOcAsync(Stream attachmentContent, string gmailId, string attachmentName, string supplierCode);
    }
}
