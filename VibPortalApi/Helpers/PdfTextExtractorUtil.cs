using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

public static class PdfTextExtractorUtil
{
    public static string ExtractTextFromPdf(string path)
    {
        var sb = new StringBuilder();

        using (var pdfReader = new PdfReader(path))
        using (var pdfDoc = new PdfDocument(pdfReader))
        {
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                var strategy = new LocationTextExtractionStrategy();
                string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                sb.AppendLine(pageText);
            }
        }

        return sb.ToString();
    }
}
