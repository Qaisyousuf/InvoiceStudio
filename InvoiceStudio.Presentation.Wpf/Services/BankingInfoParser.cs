using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.Services
{
    public class BankingInfoParser : IBankingInfoParser
    {
        private readonly ILogger _logger;

        // IBAN pattern: 2 letters + 2 digits + up to 30 alphanumeric characters
        private static readonly Regex IbanRegex = new(@"\b([A-Z]{2}[0-9]{2}[A-Z0-9]{4,30})\b", RegexOptions.IgnoreCase);

        // SWIFT/BIC pattern: 4 letters + 2 letters + 2 alphanumeric + optional 3 alphanumeric
        private static readonly Regex SwiftRegex = new(@"\b([A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?)\b", RegexOptions.IgnoreCase);

        // French RIB pattern: Bank code + Branch code + Account number + Key
        private static readonly Regex RibRegex = new(@"\b(\d{5})\s*(\d{5})\s*([A-Z0-9]{11})\s*(\d{2})\b", RegexOptions.IgnoreCase);

        // Common bank name patterns
        private static readonly Dictionary<string, string[]> BankNamePatterns = new()
        {
            ["BNP Paribas"] = new[] { "bnp", "paribas", "bnp paribas" },
            ["Crédit Agricole"] = new[] { "credit agricole", "crédit agricole", "ca" },
            ["La Banque Postale"] = new[] { "banque postale", "la banque postale", "labanquepostale", "poste" },
            ["Société Générale"] = new[] { "societe generale", "société générale", "sg" },
            ["Banque Populaire"] = new[] { "banque populaire", "bp" },
            ["Crédit Mutuel"] = new[] { "credit mutuel", "crédit mutuel", "cm" },
            ["Caisse d'Epargne"] = new[] { "caisse epargne", "caisse d'epargne", "ce" },
            ["LCL"] = new[] { "lcl", "credit lyonnais", "crédit lyonnais" },
            ["HSBC"] = new[] { "hsbc" },
            ["ING"] = new[] { "ing" },
            ["Boursorama"] = new[] { "boursorama" },
            ["Revolut"] = new[] { "revolut" },
            ["N26"] = new[] { "n26" },
            ["Danske Bank"] = new[] { "danske", "danske bank" },
            ["Nordea"] = new[] { "nordea" },
            ["Jyske Bank"] = new[] { "jyske", "jyske bank" }
        };

        public BankingInfoParser(ILogger logger)
        {
            _logger = logger;
        }

        public BankingInfoResult ParseBankingInfo(string extractedText)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return new BankingInfoResult { ConfidenceScore = 0f };
            }

            _logger.Information("Starting banking info parsing for {TextLength} characters", extractedText.Length);

            var result = new BankingInfoResult();

            // Clean and normalize text
            string cleanText = CleanText(extractedText);

            // Extract IBANs
            result.AllIbansFound = ExtractIbans(cleanText);
            result.Iban = result.AllIbansFound.FirstOrDefault();

            // Extract SWIFT/BIC codes
            result.AllSwiftCodesFound = ExtractSwiftCodes(cleanText);
            result.Swift = result.AllSwiftCodesFound.FirstOrDefault();
            result.Bic = result.Swift; // SWIFT and BIC are often the same

            // Extract bank name
            result.BankName = ExtractBankName(cleanText);

            // Extract French RIB if present
            ExtractFrenchRib(cleanText, result);

            // Calculate confidence score
            result.ConfidenceScore = CalculateConfidence(result);

            LogResults(result);

            return result;
        }

        private string CleanText(string text)
        {
            // Remove excessive whitespace and normalize
            text = Regex.Replace(text, @"\s+", " ");

            // REMOVED destructive character replacements that were breaking IBANs
            // DON'T convert 0 to O - this destroys banking data!

            return text.Trim();
        }

        private List<string> ExtractIbans(string text)
        {
            var ibans = new List<string>();

            // Enhanced IBAN pattern to handle spaces within IBANs (French format)
            var enhancedIbanPattern = @"\b(FR\s*\d{2}\s*\d{4}\s*\d{4}\s*\d{4}\s*\d{4}\s*\d{4}\s*\d{3})\b";
            var enhancedMatches = Regex.Matches(text, enhancedIbanPattern, RegexOptions.IgnoreCase);

            foreach (Match match in enhancedMatches)
            {
                string iban = match.Value.Replace(" ", "").ToUpperInvariant();
                if (IsValidIbanFormat(iban))
                {
                    string formattedIban = FormatIban(iban);
                    ibans.Add(formattedIban);
                    _logger.Debug("Found IBAN (enhanced): {IBAN}", formattedIban);
                }
            }

            // Fallback to original pattern if enhanced didn't find anything
            if (!ibans.Any())
            {
                var matches = IbanRegex.Matches(text);
                foreach (Match match in matches)
                {
                    string iban = match.Value.Replace(" ", "").Replace("-", "").ToUpperInvariant();

                    if (IsValidIbanFormat(iban))
                    {
                        string formattedIban = FormatIban(iban);
                        ibans.Add(formattedIban);
                        _logger.Debug("Found IBAN (fallback): {IBAN}", formattedIban);
                    }
                }
            }

            return ibans.Distinct().ToList();
        }

        private List<string> ExtractSwiftCodes(string text)
        {
            var swiftCodes = new List<string>();

            // Enhanced pattern to handle spaced SWIFT codes like "P S S T F R P P C L E"
            var spacedSwiftPattern = @"\b([A-Z]\s*[A-Z]\s*[A-Z]\s*[A-Z]\s*[A-Z]\s*[A-Z]\s*[A-Z0-9]\s*[A-Z0-9](\s*[A-Z0-9]\s*[A-Z0-9]\s*[A-Z0-9])?)\b";
            var spacedMatches = Regex.Matches(text, spacedSwiftPattern, RegexOptions.IgnoreCase);

            foreach (Match match in spacedMatches)
            {
                string swift = match.Value.Replace(" ", "").ToUpperInvariant();
                if (swift.Length >= 8 && swift.Length <= 11)
                {
                    swiftCodes.Add(swift);
                    _logger.Debug("Found SWIFT/BIC (spaced): {SWIFT}", swift);
                }
            }

            // Original pattern for non-spaced SWIFT codes
            var matches = SwiftRegex.Matches(text);
            foreach (Match match in matches)
            {
                string swift = match.Value.Replace(" ", "").ToUpperInvariant();

                if (swift.Length >= 8 && swift.Length <= 11)
                {
                    swiftCodes.Add(swift);
                    _logger.Debug("Found SWIFT/BIC (normal): {SWIFT}", swift);
                }
            }

            return swiftCodes.Distinct().ToList();
        }

        private string? ExtractBankName(string text)
        {
            string lowerText = text.ToLowerInvariant();

            foreach (var bank in BankNamePatterns)
            {
                foreach (var pattern in bank.Value)
                {
                    if (lowerText.Contains(pattern))
                    {
                        _logger.Debug("Found bank name: {BankName} (matched pattern: {Pattern})", bank.Key, pattern);
                        return bank.Key;
                    }
                }
            }

            // Try to extract bank name from IBAN prefix for common French banks
            var ibans = ExtractIbans(text);
            if (ibans.Any())
            {
                string? bankFromIban = GetBankFromIbanPrefix(ibans.First());
                if (!string.IsNullOrEmpty(bankFromIban))
                {
                    _logger.Debug("Identified bank from IBAN: {BankName}", bankFromIban);
                    return bankFromIban;
                }
            }

            return null;
        }

        private void ExtractFrenchRib(string text, BankingInfoResult result)
        {
            // Enhanced RIB pattern to handle account numbers with letters (like U)
            var enhancedRibPattern = @"\b(\d{5})\s*(\d{5})\s*([A-Z0-9]{10,11})\s*(\d{2})\b";
            var matches = Regex.Matches(text, enhancedRibPattern, RegexOptions.IgnoreCase);

            if (matches.Count > 0)
            {
                var match = matches[0];
                string bankCode = match.Groups[1].Value;
                string branchCode = match.Groups[2].Value;
                string accountNumber = match.Groups[3].Value;
                string key = match.Groups[4].Value;

                result.AccountNumber = $"{bankCode} {branchCode} {accountNumber} {key}";
                result.AdditionalInfo["FrenchRIB"] = new
                {
                    BankCode = bankCode,
                    BranchCode = branchCode,
                    AccountNumber = accountNumber,
                    Key = key
                };

                _logger.Debug("Found French RIB: {RIB}", result.AccountNumber);
            }
        }

        private bool IsValidIbanFormat(string iban)
        {
            if (string.IsNullOrEmpty(iban) || iban.Length < 15 || iban.Length > 34)
                return false;

            // Check if it starts with 2 letters followed by 2 digits
            if (!char.IsLetter(iban[0]) || !char.IsLetter(iban[1]) ||
                !char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
                return false;

            return true;
        }

        private string FormatIban(string iban)
        {
            // Format IBAN with spaces every 4 characters
            return string.Join(" ", Enumerable.Range(0, (iban.Length + 3) / 4)
                .Select(i => iban.Substring(i * 4, Math.Min(4, iban.Length - i * 4))));
        }

        private string? GetBankFromIbanPrefix(string iban)
        {
            if (string.IsNullOrEmpty(iban) || iban.Length < 6)
                return null;

            string cleanIban = iban.Replace(" ", "");

            // French bank codes (first 5 digits after FR)
            if (cleanIban.StartsWith("FR", StringComparison.OrdinalIgnoreCase))
            {
                string bankCode = cleanIban.Substring(4, Math.Min(5, cleanIban.Length - 4));

                return bankCode switch
                {
                    "30004" => "BNP Paribas",
                    "30002" => "Crédit Agricole",
                    "30003" => "Société Générale",
                    "16808" => "Banque Populaire",
                    "10278" => "Crédit Mutuel",
                    "20041" => "La Banque Postale",
                    "30001" => "LCL",
                    "28233" => "Revolut",
                    _ => null
                };
            }

            return null;
        }

        private float CalculateConfidence(BankingInfoResult result)
        {
            float score = 0f;

            if (!string.IsNullOrEmpty(result.Iban)) score += 0.4f;
            if (!string.IsNullOrEmpty(result.Swift)) score += 0.3f;
            if (!string.IsNullOrEmpty(result.BankName)) score += 0.2f;
            if (!string.IsNullOrEmpty(result.AccountNumber)) score += 0.1f;

            return Math.Min(1.0f, score);
        }

        private void LogResults(BankingInfoResult result)
        {
            _logger.Information("Banking info parsing completed with confidence: {Confidence:F2}%", result.ConfidenceScore * 100);

            if (!string.IsNullOrEmpty(result.BankName))
                _logger.Information("Bank Name: {BankName}", result.BankName);

            if (!string.IsNullOrEmpty(result.Iban))
                _logger.Information("IBAN: {IBAN}", result.Iban);

            if (!string.IsNullOrEmpty(result.Swift))
                _logger.Information("SWIFT/BIC: {SWIFT}", result.Swift);

            if (result.AllIbansFound.Count > 1)
                _logger.Information("Additional IBANs found: {AdditionalIBANs}", string.Join(", ", result.AllIbansFound.Skip(1)));
        }
    }
}