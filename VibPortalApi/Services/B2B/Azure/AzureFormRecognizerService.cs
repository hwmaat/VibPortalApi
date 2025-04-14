using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace VibPortalApi.Services.B2B.Azure
{
    public class AzureFormRecognizerService
    {
        private readonly DocumentAnalysisClient _client;

        public AzureFormRecognizerService(string endpoint, string apiKey)
        {
            var credential = new AzureKeyCredential(apiKey);
            _client = new DocumentAnalysisClient(new Uri(endpoint), credential);
        }

        public async Task<string> AnalyzePdfAsync(Stream pdfStream)
        {
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", pdfStream);
            var result = operation.Value;

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("📌 Key-Value Pairs:");
            foreach (var kv in result.KeyValuePairs)
            {
                sb.AppendLine($"  🔑 {kv.Key?.Content} → 📄 {kv.Value?.Content}");
            }

            sb.AppendLine("\n📊 Tables:");
            foreach (var table in result.Tables)
            {
                sb.AppendLine($"- Table with {table.RowCount} rows and {table.ColumnCount} columns");
                for (int i = 0; i < table.RowCount; i++)
                {
                    for (int j = 0; j < table.ColumnCount; j++)
                    {
                        var cell = table.Cells.FirstOrDefault(c => c.RowIndex == i && c.ColumnIndex == j);
                        sb.Append($"{cell?.Content ?? ""} | ");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
