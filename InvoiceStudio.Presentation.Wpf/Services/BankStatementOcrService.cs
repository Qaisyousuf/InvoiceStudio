using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PdfiumViewer;
using Serilog;
using Tesseract;

namespace InvoiceStudio.Presentation.Wpf.Services
{
    public class BankStatementOcrService : IBankStatementOcrService
    {
        private readonly ILogger _logger;
        private readonly string _tessDataPath;

        public BankStatementOcrService(ILogger logger)
        {
            _logger = logger;

            // Set up Tesseract data path - this should point to tessdata folder
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            // Create tessdata directory if it doesn't exist
            Directory.CreateDirectory(_tessDataPath);

            _logger.Information("BankStatementOcrService initialized with tessdata path: {TessDataPath}", _tessDataPath);
        }

        public async Task<string> ExtractTextAsync(string filePath, Action<string>? progressCallback = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            if (!IsSupportedFormat(filePath))
            {
                throw new NotSupportedException($"File format not supported: {Path.GetExtension(filePath)}");
            }

            try
            {
                _logger.Information("Starting OCR extraction for file: {FilePath}", filePath);
                progressCallback?.Invoke("Initializing OCR engine...");

                string extractedText;
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                if (extension == ".pdf")
                {
                    extractedText = await ExtractFromPdfAsync(filePath, progressCallback);
                }
                else
                {
                    extractedText = await ExtractFromImageAsync(filePath, progressCallback);
                }

                _logger.Information("OCR extraction completed. Extracted {CharCount} characters", extractedText.Length);
                progressCallback?.Invoke("Text extraction completed!");

                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to extract text from file: {FilePath}", filePath);
                throw;
            }
        }

        public bool IsSupportedFormat(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => true,
                ".png" => true,
                ".jpg" => true,
                ".jpeg" => true,
                ".bmp" => true,
                ".tiff" => true,
                ".tif" => true,
                _ => false
            };
        }

        private async Task<string> ExtractFromPdfAsync(string pdfPath, Action<string>? progressCallback)
        {
            progressCallback?.Invoke("Loading PDF document...");

            using var document = PdfDocument.Load(pdfPath);
            var extractedText = new StringBuilder();

            _logger.Information("PDF loaded with {PageCount} pages", document.PageCount);

            for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
            {
                progressCallback?.Invoke($"Processing page {pageIndex + 1} of {document.PageCount}...");

                try
                {
                    // Convert PDF page to image (returns System.Drawing.Image)
                    using var pageImage = document.Render(pageIndex, 300, 300, PdfRenderFlags.CorrectFromDpi);

                    // Convert Image to Bitmap
                    using var bitmap = new Bitmap(pageImage);

                    // Extract text from the bitmap
                    string pageText = await ExtractTextFromBitmap(bitmap, progressCallback);
                    extractedText.AppendLine(pageText);

                    _logger.Debug("Extracted {CharCount} characters from page {PageIndex}", pageText.Length, pageIndex + 1);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to process page {PageIndex}", pageIndex + 1);
                    // Continue with other pages
                }
            }

            return extractedText.ToString();
        }

        private async Task<string> ExtractFromImageAsync(string imagePath, Action<string>? progressCallback)
        {
            progressCallback?.Invoke("Loading image file...");

            using var bitmap = new Bitmap(imagePath);
            return await ExtractTextFromBitmap(bitmap, progressCallback);
        }

        private async Task<string> ExtractTextFromBitmap(Bitmap bitmap, Action<string>? progressCallback)
        {
            return await Task.Run(() =>
            {
                progressCallback?.Invoke("Running OCR analysis...");

                try
                {
                    // Check if tessdata exists and has required files
                    EnsureTessDataExists();

                    using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);

                    // Configure OCR settings for better banking document recognition
                    engine.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .-/");
                    engine.SetVariable("preserve_interword_spaces", "1");

                    progressCallback?.Invoke("Analyzing document structure...");

                    // Create Pix from bitmap data
                    using var pix = Pix.LoadFromMemory(BitmapToByteArray(bitmap));
                    using var page = engine.Process(pix, PageSegMode.Auto);

                    string text = page.GetText();
                    float confidence = page.GetMeanConfidence();

                    _logger.Information("OCR completed with confidence: {Confidence:F2}%", confidence * 100);

                    if (confidence < 0.3f)
                    {
                        _logger.Warning("Low OCR confidence: {Confidence:F2}%. Results may be inaccurate.", confidence * 100);
                    }

                    return text ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "OCR processing failed");
                    throw;
                }
            });
        }

        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        private void EnsureTessDataExists()
        {
            string engTrainedDataPath = Path.Combine(_tessDataPath, "eng.traineddata");

            if (!File.Exists(engTrainedDataPath))
            {
                string message = $"Tesseract English language data not found at: {engTrainedDataPath}\n\n" +
                               "Please download 'eng.traineddata' from:\n" +
                               "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata\n\n" +
                               "And place it in the tessdata folder.";

                _logger.Error("Missing Tesseract language data: {Path}", engTrainedDataPath);
                throw new FileNotFoundException(message);
            }
        }
    }
}