using System.Text.RegularExpressions;
using VibPortalApi.Models.Vib;

namespace VibPortalApi.Services.Euravib
{
    public class PdfExtractor_Beckers : IPdfExtractorService
    {
        public VibImport ExtractData(string filePath)
        {
            //using var document = PdfDocument.Open(filePath);
            //var text = string.Join("\n", document.GetPages().Select(p => p.Text));

            var text = PdfTextExtractorUtil.ExtractTextFromPdf(filePath);

            var (casNumbers, casPercentages) = ExtractCasData(text);
            var (hNumbers, hCats) = ExtractHazardCodesAndCategories(text);
            var (unNummer, ladingNaam, gevarenKlasse, verpakkingsGroep, milieugevaren) = ExtractTransportInfo(text);

            var (Imdg_unNummer, Imdg_ladingNaam, Imdg_gevarenKlasse, Imdg_verpakkingsGroep, Imdg_milieugevaren) = ExtractImdgInfo(text);
            var vlamPunt = ExtractVlampunt(text);
            var tunnelCode = ExtractTunnelCode(text);


            var result = new VibImport
            {
                Rev_Date = ExtractRevisionDate(text),
                Cas_Nr = casNumbers,
                Cas_Perc = casPercentages,
                FlashPoint = ExtractFlashPoint(text),
                Suppl_Nr = "",
                Dimset = "",
                H_Nr = hNumbers,
                H_Cat = hCats,
                Adr_Un_Nr = unNummer,
                Adr_Cargo_Name = ladingNaam,
                Adr_TransportHazard_Class = gevarenKlasse,
                Adr_Packaging_Group = verpakkingsGroep,
                Adr_Environment_Hazards = milieugevaren,
                Adr_ExtraInfo = "",
                Imdg_Un_Nr = Imdg_unNummer,
                Imdg_Cargo_Name = Imdg_ladingNaam,
                Imdg_TransportHazard_Class = Imdg_gevarenKlasse,
                Imdg_Packaging_Group = Imdg_verpakkingsGroep,
                Imdg_Environment_Hazards = Imdg_milieugevaren,
                Imdg_ExtraInfo = "",
                ExtraInfo_TunnelCode = tunnelCode,
                Ems_Fire = "",
                Ems_Spillage = "",
                Eg_Nr = "",
            };

            return result;
        }

        public static DateTime? ExtractRevisionDate(string text)
        {
            // Probeer match met gecombineerde regel (meest betrouwbaar)
            var match = Regex.Match(text, @"Datum van uitgave/\s*Revisie datum\s*:\s*(?<revDate>\d{1,2}/\d{1,2}/\d{4})", RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Groups["revDate"].Value, out var parsed1))
                return parsed1;

            // Alternatief: zoeken naar losse lijn met alleen Revisie datum
            match = Regex.Match(text, @"Revisie datum\s*[:\-]?\s*(?<revDate>\d{1,2}/\d{1,2}/\d{4})", RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Groups["revDate"].Value, out var parsed2))
                return parsed2;

            // Als fallback: pak eerste match van "Revisie datum" gevolgd door een datum ergens in de buurt
            match = Regex.Match(text, @"Revisie datum.*?(?<revDate>\d{1,2}/\d{1,2}/\d{4})", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success && DateTime.TryParse(match.Groups["revDate"].Value, out var parsed3))
                return parsed3;

            return null;
        }

        private string? ExtractVlampunt(string text)
        {
            var section9 = ExtractRubriekSection(text, 9);
            if (string.IsNullOrWhiteSpace(section9))
                return null;

            // Zoek naar Vlampunt gevolgd door een temperatuur (bijv. 45°C of 45 C)
            var match = Regex.Match(section9, @"Vlampunt\s*:\s*[^:]*?(\d{1,3}\s*°?C?)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        private string? ExtractTunnelCode(string text)
        {
            var section14 = ExtractRubriekSection(text, 14);
            if (string.IsNullOrWhiteSpace(section14))
                return null;

            // Zoek naar iets als: Tunnelcode (D/E) → match 'D/E'
            var match = Regex.Match(section14, @"Tunnelcode\s*\(([^)]+)\)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }
        private (string casNumbers, string casPercentages) ExtractCasData(string text)
        {
            var rubriek3Text = ExtractRubriekSection(text, 3);
            var lines = rubriek3Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var casList = new List<string>();
            var casRegex = new Regex(@"\b\d{2,7}-\d{2}-\d\b");

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("CAS-nummer", StringComparison.OrdinalIgnoreCase))
                {
                    // Check op dezelfde regel
                    var matchOnLine = casRegex.Match(lines[i]);
                    if (matchOnLine.Success)
                    {
                        casList.Add(matchOnLine.Value);
                        continue;
                    }

                    // Check op de volgende 1 of 2 regels
                    for (int j = 1; j <= 2 && i + j < lines.Length; j++)
                    {
                        var match = casRegex.Match(lines[i + j]);
                        if (match.Success)
                        {
                            casList.Add(match.Value);
                            break;
                        }
                    }
                }
            }

            // Percentages
            var percRegex = new Regex(@"≥\s*([\d.,]+)\s*[-–]\s*[<|≤]\s*([\d.,]+)", RegexOptions.IgnoreCase);
            var percMatches = percRegex.Matches(rubriek3Text);
            var percList = percMatches.Cast<Match>()
                .Select(m => $"≥{m.Groups[1].Value.Trim()} - ≤{m.Groups[2].Value.Trim()}")
                .ToList();

            int count = Math.Min(casList.Count, percList.Count);
            var casNumbers = string.Join("|", casList.Take(count));
            var casPercentages = string.Join("|", percList.Take(count));

            return (casNumbers, casPercentages);
        }

        private string? ExtractFlashPoint(string text)
        {
            var section9 = ExtractRubriekSection(text, 9);
            if (string.IsNullOrWhiteSpace(section9))
                return null;

            // Match volledige waarde achter "Vlampunt :" tot aan newline of einde
            var match = Regex.Match(section9, @"Vlampunt\s*:\s*(.+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        private (string H_Nr, string H_Cat) ExtractHazardCodesAndCategories(string fullText)
        {
            var hNrList = new List<string>();
            var hCatList = new List<string>();

            // Extract Rubriek 2 (Gevarenidentificatie)
            string section2 = ExtractRubriekSection(fullText, 2);

            // Regex voor regels zoals: Flam. Liq. 3, H226 of Acute Tox. 4, H302
            var lineRegex = new Regex(@"(?<cat>[A-Za-z\s\.]+?\d?),\s*(?<code>H\d{3})", RegexOptions.Multiline);

            foreach (Match match in lineRegex.Matches(section2))
            {
                if (match.Success)
                {
                    string hCode = match.Groups["code"].Value.Trim();
                    string hCat = match.Groups["cat"].Value.Trim();

                    if (!hNrList.Contains(hCode))
                    {
                        hNrList.Add(hCode);
                        hCatList.Add(hCat);
                    }
                }
            }

            return (
                string.Join("|", hNrList),
                string.Join("|", hCatList)
            );
        }

        private (string? UN_Nummer, string? Ladingnaam, string? Gevarenklasse, string? Verpakkingsgroep, string? Milieugevaren) ExtractTransportInfo(string text)
        {
            string? unNummer = null;
            string? ladingnaam = null;
            string? gevarenklasse = null;
            string? verpakkingsgroep = null;
            string? milieugevaren = null;

            var section14Text = ExtractRubriekSection(text, 14);
            if (string.IsNullOrWhiteSpace(section14Text))
                return (null, null, null, null, null);

            // UN-nummer
            var unMatch = Regex.Match(section14Text, @"\bUN\d{4}\b", RegexOptions.IgnoreCase);
            if (unMatch.Success)
                unNummer = unMatch.Value;

            // Ladingnaam
            var nameMatch = Regex.Match(section14Text, @"\bVERF\b|\bPAINT\b", RegexOptions.IgnoreCase);
            if (nameMatch.Success)
                ladingnaam = nameMatch.Value;

            // Gevarenklasse: Zoek naar regel met "14.3" en klasse (zoals "14.3 3")
            var classMatch = Regex.Match(section14Text, @"14\.3\s+(\d{1,2})", RegexOptions.IgnoreCase);
            if (classMatch.Success)
                gevarenklasse = classMatch.Groups[1].Value;

            // Verpakkingsgroep: Zoek naar "14.4 III" of "Verpakkingsgroep.*III"
            var packMatch = Regex.Match(section14Text, @"14\.4\s+(I{1,3})\b", RegexOptions.IgnoreCase);
            if (!packMatch.Success)
                packMatch = Regex.Match(section14Text, @"Verpakkingsgroep.*?\b(I{1,3})\b", RegexOptions.IgnoreCase);
            if (packMatch.Success)
                verpakkingsgroep = packMatch.Groups[1].Value;

            // Milieugevaren: Ja of Nee
            var envMatch = Regex.Match(section14Text, @"Milieugevaren.*?\b(Ja|Nee)\b", RegexOptions.IgnoreCase);
            if (envMatch.Success)
                milieugevaren = envMatch.Groups[1].Value;

            return (unNummer, ladingnaam, gevarenklasse, verpakkingsgroep, milieugevaren);
        }

        private (string? UN_Nummer, string? Ladingnaam, string? Gevarenklasse, string? Verpakkingsgroep, string? Milieugevaren) ExtractImdgInfo(string text)
        {
            string? unNummer = null;
            string? ladingnaam = null;
            string? gevarenklasse = null;
            string? verpakkingsgroep = null;
            string? milieugevaren = null;

            var section14 = ExtractRubriekSection(text, 14);
            if (string.IsNullOrWhiteSpace(section14))
                return (null, null, null, null, null);

            var lines = section14.Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.StartsWith("14.") || l.StartsWith("ADR/RID"))
                .ToList();

            foreach (var line in lines)
            {
                var parts = Regex.Split(line, @"\s+");
                if (parts.Length < 3)
                    continue;

                if (line.StartsWith("14.1"))
                    unNummer = parts[3]; // 2 = IMDG-kolom
                else if (line.StartsWith("14.2"))
                    ladingnaam = parts[3];
                else if (line.StartsWith("14.3"))
                    gevarenklasse = parts[3];
                else if (line.StartsWith("14.4"))
                    verpakkingsgroep = parts[3];
                else if (line.StartsWith("14.5"))
                    milieugevaren = parts[3];
            }

            return (unNummer, ladingnaam, gevarenklasse, verpakkingsgroep, milieugevaren);
        }


        private string ExtractRubriekSection(string text, int rubriekNumber)
        {
            // Rubriek 0: alles vóór de eerste echte rubriek (1)
            if (rubriekNumber == 0)
            {
                // Toestaan dat RUBRIEK voorafgegaan wordt door tekst zonder spatie
                var firstRubriek = Regex.Match(text, @"(?i)(.{0,40})RUBRIEK\s*1\s*:");
                return firstRubriek.Success
                    ? text.Substring(0, firstRubriek.Index).Trim()
                    : text.Trim();
            }

            // Zoekstart voor "RUBRIEK X", zelfs als eraan vastgeplakt
            var startMatch = Regex.Match(text, $@"(?i)(^|\W)\w*RUBRIEK\s*{rubriekNumber}\s*:");
            if (!startMatch.Success)
                return "";

            int startIndex = startMatch.Index + startMatch.Length;

            int endIndex = text.Length;
            if (rubriekNumber < 16)
            {
                var endMatch = Regex.Match(text, $@"(?i)(^|\W)\w*RUBRIEK\s*{rubriekNumber + 1}\s*:");
                if (endMatch.Success)
                    endIndex = endMatch.Index;
            }

            if (endIndex <= startIndex)
                return "";

            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }









    }
}
