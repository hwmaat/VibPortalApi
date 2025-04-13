using VibPortalApi.Models.Zenya;

namespace VibPortalApi.Services.Zenya
{
    public interface IZenyaService
    {
        Task<string?> GetAccessTokenAsync();
        Task<ZenyaVersion?> GetVersionAsync();
        Task<string?> SetDocumentFilterAsync(string title, int folderId);
        Task<List<ZenyaDocument>?> GetDocumentsByFilterAsync(string filterId);
        Task<List<ZenyaDocument>?> SearchDocumentAsync(string title, int folderId);

    }
}
