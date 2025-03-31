using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using VibPortalApi.Models;

namespace VibPortalApi.Services
{
    public class PdfExtractor_Gardobond : IPdfExtractorService
    {
        public VibImport ExtractData(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            var text = string.Join("\n", document.GetPages().Select(p => p.Text));

            var result = new VibImport
            {
                RevisionDate = ExtractRevisionDate(text),
                Cas_Number = ExtractCasNumbers(text),
                Cas_Percentages = ExtractCasPercentages(text),
                FlashPoint = ExtractFlashPoint(text),

                // The rest will be filled in later
                SupplierNr = "",
                Dimset = "",
                EntryDate = DateTime.Now,
                H_Number = ExtractHNumbers(text),
                H_Cat = "",
                Adr_Un_Nr = "",
                Adr_CargoName = "",
                Adr_TransportHazardClass = "",
                Adr_PackagingGroup = "",
                Adr_EnvironmentHazards = "",
                Adr_ExtraInfo = "",
                Imdg_UnNumber = "",
                Imdg_CargoName = "",
                Imdg_TransportHazardClass = "",
                Imdg_PackagingGroup = "",
                Imdg_EnvironmentHazards = "",
                Imdg_ExtraInfo = "",
                ExtraInfoTunnelCode = "",
                Ems_Fire = "",
                Ems_Spillage = "",
                UserName = "",
                EgNumber = "",
                Status = ""
            };

            return result;
        }

        private DateTime? ExtractRevisionDate(string text)
        {
            var section = ExtractRubriekSection(text, 0);
            var match = Regex.Match(section, @"Herzieningsdatum:\s*(\d{2}\.\d{2}\.\d{4})");
            if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }

        private string ExtractCasNumbers(string text)
        {
            var section = ExtractRubriekSection(text, 3);
            var casMatches = Regex.Matches(section, @"\b\d{2,7}-\d{2}-\d\b");
            return string.Join(", ", casMatches);
        }

        private string ExtractCasPercentages(string text)
        {
            var section = ExtractRubriekSection(text, 3);
            var percentMatches = Regex.Matches(section, @">=\s*\d+\s*-\s*<\s*\d+").Cast<Match>();
            return string.Join(", ", percentMatches);
        }

        private string ExtractFlashPoint(string text)
        {
            var section = ExtractRubriekSection(text, 9);
            var match = Regex.Match(section, @"Zelfontbrandingstemperatu\s*:\s*(.+?)\s*(?:\r|\n|$)");
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private string ExtractHNumbers(string text)
        {
            var section = ExtractRubriekSection(text, 2);

            var matches = Regex.Matches(section, @"\bH\d{3}\b", RegexOptions.IgnoreCase);

            return string.Join(", ",
                matches.Cast<Match>()
                       .Select(m => m.Value.ToUpper())
                       .Distinct());
        }


        private string ExtractRubriekSection(string text, int rubriekNumber)
        {
            // Special case: everything before RUBRIEK 1
            if (rubriekNumber == 0)
            {
                var firstRubriek = Regex.Match(text, @"\bRUBRIEK\s+1\s*:", RegexOptions.IgnoreCase);
                return firstRubriek.Success
                    ? text.Substring(0, firstRubriek.Index).Trim()
                    : text.Trim();
            }

            // Match real heading "RUBRIEK {X}:" with a word boundary
            var startMatch = Regex.Match(text, $@"\bRUBRIEK\s+{rubriekNumber}\s*:", RegexOptions.IgnoreCase);
            if (!startMatch.Success)
                return "";

            int startIndex = startMatch.Index + startMatch.Length;

            int endIndex = text.Length;
            if (rubriekNumber < 16)
            {
                var endMatch = Regex.Match(text, $@"\bRUBRIEK\s+{rubriekNumber + 1}\s*:", RegexOptions.IgnoreCase);
                if (endMatch.Success)
                    endIndex = endMatch.Index;
            }

            if (endIndex <= startIndex)
                return "";

            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }







    }
}
