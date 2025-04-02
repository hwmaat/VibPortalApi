using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public interface IZenyaService
    {
        Task<string?> GetSearchSuggestionAsync(string query);
        Task<string?> GetAccessTokenAsync();
        Task<ZenyaVersion?> GetVersionAsync();
        Task<string?> SetDocumentFilterAsync(string title, int folderId);
        Task<List<ZenyaDocument>?> GetDocumentsByFilterAsync(string filterId);
        Task<List<ZenyaDocument>?> SearchDocumentAsync(string title, int folderId);

    }

    public class ZenyaVersion
    {
        public string name { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
    }

    public class ZenyaService : IZenyaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly string _baseUrl;
        private readonly string _username;
        private string? _cachedToken;
        private DateTime _tokenRetrievedAt;

        public ZenyaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["Zenya:ApiToken"] ?? throw new ArgumentNullException("Zenya:ApiToken is missing");
            _baseUrl = configuration["Zenya:BaseUrl"] ?? throw new ArgumentNullException("Zenya:BaseUrl is missing");
            _username = configuration["Zenya:Username"] ?? throw new ArgumentNullException("Zenya:Username is missing");
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow - _tokenRetrievedAt < TimeSpan.FromMinutes(55))
            {
                return _cachedToken;
            }

            var tokenUrl = $"{_baseUrl.TrimEnd('/')}/tokens";

            var payload = new
            {
                api_key = _apiToken,
                username = _username
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(tokenUrl, content);
            if (!response.IsSuccessStatusCode)
                return null;

            var tokenString = await response.Content.ReadAsStringAsync();

            try
            {
                _cachedToken = JsonSerializer.Deserialize<string>(tokenString);
                _tokenRetrievedAt = DateTime.UtcNow;
                return _cachedToken;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetSearchSuggestionAsync(string query)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var url = $"{_baseUrl.TrimEnd('/')}/portals/105/search_suggestions?search_query={Uri.EscapeDataString(query)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Authorization", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("x-api-version", "5");

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var title = root.EnumerateArray().FirstOrDefault().GetProperty("title").GetString();
            return title;
        }
        public async Task<ZenyaVersion?> GetVersionAsync()
        {
            var url = $"{_baseUrl.TrimEnd('/')}/versions/iprova";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("x-api-version", "5");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var version = JsonSerializer.Deserialize<ZenyaVersion>(json);
            return version;
        }

        public async Task<string?> SetDocumentFilterAsync(string title, int folderId)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var url = $"{_baseUrl.TrimEnd('/')}/documents/filter";

            var payload = new
            {
                title = new { op = "co", value = title },
                folder_ids = new[] { folderId }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "91bfb9e861e34452947b6a9e4a2f76da");
            request.Headers.Add("Authorization", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("x-api-version", "5");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string>(result);
           
        }

        public async Task<List<ZenyaDocument>?> GetDocumentsByFilterAsync(string filterId)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var url = $"{_baseUrl.TrimEnd('/')}/documents/filter/{filterId}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("x-api-version", "5");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ZenyaDocument>>(json);
        }

        public async Task<List<ZenyaDocument>?> SearchDocumentAsync(string title, int folderId)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            // Stap 1: filter plaatsen
            var filterUrl = $"{_baseUrl.TrimEnd('/')}/documents/filter";
            var payload = new
            {
                title = new { op = "co", value = title },
                folder_ids = new[] { folderId }
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var filterRequest = new HttpRequestMessage(HttpMethod.Post, filterUrl)
            {
                Content = content
            };
            //00520 70R1071.25
            filterRequest.Headers.TryAddWithoutValidation("Authorization", token);
            filterRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            filterRequest.Headers.Add("x-api-version", "5");

            using var filterResponse = await _httpClient.SendAsync(filterRequest);
            filterResponse.EnsureSuccessStatusCode();

            var filterResult = await filterResponse.Content.ReadAsStringAsync();
            var filterId = JsonSerializer.Deserialize<string>(filterResult);

            if (string.IsNullOrEmpty(filterId)) return null;

            // Stap 2: documenten ophalen
            var getUrl = $"{_baseUrl.TrimEnd('/')}/documents/filter/{filterId}";
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, getUrl);

            //getRequest.Headers.TryAddWithoutValidation("Authorization", token);
            //getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            getRequest.Headers.Add("x-api-version", "5");

            using var getResponse = await _httpClient.SendAsync(getRequest);
            getResponse.EnsureSuccessStatusCode();

            var str = getResponse.Content.ReadAsStringAsync();

            var documentsJson = await getResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ZenyaDocument>>(documentsJson);
        }

    }
}