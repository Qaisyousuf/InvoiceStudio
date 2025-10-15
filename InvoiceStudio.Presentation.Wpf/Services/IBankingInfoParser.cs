using System.Collections.Generic;

namespace InvoiceStudio.Presentation.Wpf.Services
{
    public interface IBankingInfoParser
    {
        /// <summary>
        /// Parses extracted text to find banking information
        /// </summary>
        /// <param name="extractedText">OCR extracted text from bank statement</param>
        /// <returns>Dictionary of found banking information</returns>
        BankingInfoResult ParseBankingInfo(string extractedText);
    }

    public class BankingInfoResult
    {
        public string? BankName { get; set; }
        public string? Iban { get; set; }
        public string? Swift { get; set; }
        public string? Bic { get; set; }
        public string? AccountNumber { get; set; }
        public string? SortCode { get; set; }
        public List<string> AllIbansFound { get; set; } = new();
        public List<string> AllSwiftCodesFound { get; set; } = new();
        public float ConfidenceScore { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }
}