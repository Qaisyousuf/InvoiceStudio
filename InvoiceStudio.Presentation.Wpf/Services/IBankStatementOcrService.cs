using System;
using System.Threading.Tasks;

namespace InvoiceStudio.Presentation.Wpf.Services
{
    public interface IBankStatementOcrService
    {
        /// <summary>
        /// Extracts text from a bank statement file (PDF or image)
        /// </summary>
        /// <param name="filePath">Path to the bank statement file</param>
        /// <param name="progressCallback">Optional callback to report progress</param>
        /// <returns>Extracted text content</returns>
        Task<string> ExtractTextAsync(string filePath, Action<string>? progressCallback = null);

        /// <summary>
        /// Checks if the file format is supported
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if supported, false otherwise</returns>
        bool IsSupportedFormat(string filePath);
    }
}