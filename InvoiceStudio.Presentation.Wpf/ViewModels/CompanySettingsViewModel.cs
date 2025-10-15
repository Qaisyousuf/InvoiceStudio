using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.Services;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class CompanySettingsViewModel : ViewModelBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger _logger;
    private readonly IBankStatementOcrService _ocrService;
    private readonly IBankingInfoParser _bankingInfoParser;
    private Company? _currentCompany;

    // Basic Information
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _legalName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _website = string.Empty;

    // Address
    [ObservableProperty]
    private string _street = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private string _countryName = string.Empty;

    // Business Settings
    [ObservableProperty]
    private string _country = "FR"; // Default to France

    [ObservableProperty]
    private string _legalForm = string.Empty;

    [ObservableProperty]
    private string _businessRegistrationNumber = string.Empty;

    [ObservableProperty]
    private string _taxId = string.Empty;

    [ObservableProperty]
    private string _vatNumber = string.Empty;

    [ObservableProperty]
    private bool _isVatExempt = true;

    // French-specific fields
    [ObservableProperty]
    private string _siret = string.Empty;

    [ObservableProperty]
    private string _siren = string.Empty;

    [ObservableProperty]
    private string _apeCode = string.Empty;

    [ObservableProperty]
    private string _rcsNumber = string.Empty;

    [ObservableProperty]
    private string _frenchVatExemptionMention = "TVA non applicable, art. 293 B du CGI";

    // Danish-specific fields
    [ObservableProperty]
    private string _cvrNumber = string.Empty;

    [ObservableProperty]
    private string _danishVatNumber = string.Empty;

    [ObservableProperty]
    private string _seNumber = string.Empty;

    [ObservableProperty]
    private string _intraCommunityVat = string.Empty;

    // Universal Banking Information
    [ObservableProperty]
    private string _bankName = string.Empty;

    [ObservableProperty]
    private string _iban = string.Empty;

    [ObservableProperty]
    private string _swift = string.Empty;

    // French Banking Properties (RIB)
    [ObservableProperty]
    private string _frenchBankCode = string.Empty;

    [ObservableProperty]
    private string _frenchBranchCode = string.Empty;

    [ObservableProperty]
    private string _frenchAccountNumber = string.Empty;

    [ObservableProperty]
    private string _frenchRibKey = string.Empty;

    // Danish Banking Properties
    [ObservableProperty]
    private string _danishRegistrationNumber = string.Empty;

    [ObservableProperty]
    private string _danishAccountNumber = string.Empty;

    // Insurance Information
    [ObservableProperty]
    private string _insuranceCompany = string.Empty;

    [ObservableProperty]
    private string _insurancePolicyNumber = string.Empty;

    // Branding
    [ObservableProperty]
    private string _logoPath = string.Empty;

    [ObservableProperty]
    private string _primaryColor = "#2563EB";

    // Logo Upload Properties
    [ObservableProperty]
    private bool _hasLogo = false;

    [ObservableProperty]
    private string _logoPreviewPath = string.Empty;

    // Bank Statement Scanner Properties
    [ObservableProperty]
    private bool _isProcessingStatement = false;

    [ObservableProperty]
    private bool _hasStatementFile = false;

    [ObservableProperty]
    private string _statementFileName = string.Empty;

    [ObservableProperty]
    private string _statementFilePath = string.Empty;

    [ObservableProperty]
    private string _processingStatus = string.Empty;

    // Default Settings & Invoice Configuration
    [ObservableProperty]
    private string _defaultCurrency = "EUR";

    [ObservableProperty]
    private decimal _defaultTaxRate = 20.0m;

    [ObservableProperty]
    private string _invoicePrefix = "INV";

    [ObservableProperty]
    private string _quotePrefix = "QUO";

    // UI State
    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    // Country selection for UI
    [ObservableProperty]
    private int _selectedCountryIndex = 0; // 0=France, 1=Denmark

    // Helper properties for UI visibility
    public bool ShowFrenchFields => SelectedCountryIndex == 0;
    public bool ShowDanishFields => SelectedCountryIndex == 1;
    public bool ShowFrenchBanking => SelectedCountryIndex == 0;
    public bool ShowDanishBanking => SelectedCountryIndex == 1;

    // Computed property for enabling scan button
    public bool CanScanStatement => HasStatementFile && !IsProcessingStatement;

    // Property change handlers
    partial void OnSelectedCountryIndexChanged(int value)
    {
        Country = value == 0 ? "FR" : "DK";
        DefaultCurrency = value == 0 ? "EUR" : "DKK";
        DefaultTaxRate = value == 0 ? 20.0m : 25.0m;
        OnPropertyChanged(nameof(ShowFrenchFields));
        OnPropertyChanged(nameof(ShowDanishFields));
        OnPropertyChanged(nameof(ShowFrenchBanking));
        OnPropertyChanged(nameof(ShowDanishBanking));
        MarkAsChanged();
    }

    partial void OnHasStatementFileChanged(bool value)
    {
        OnPropertyChanged(nameof(CanScanStatement));
    }

    partial void OnIsProcessingStatementChanged(bool value)
    {
        OnPropertyChanged(nameof(CanScanStatement));
    }

    // Banking validation property change handlers
    partial void OnFrenchBankCodeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^\d{0,5}$"))
        {
            FrenchBankCode = Regex.Replace(value, @"[^\d]", "").Substring(0, Math.Min(5, value.Length));
        }
    }

    partial void OnFrenchBranchCodeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^\d{0,5}$"))
        {
            FrenchBranchCode = Regex.Replace(value, @"[^\d]", "").Substring(0, Math.Min(5, value.Length));
        }
    }

    partial void OnFrenchAccountNumberChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^[A-Z0-9]{0,11}$", RegexOptions.IgnoreCase))
        {
            FrenchAccountNumber = Regex.Replace(value.ToUpper(), @"[^A-Z0-9]", "").Substring(0, Math.Min(11, value.Length));
        }
    }

    partial void OnFrenchRibKeyChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^\d{0,2}$"))
        {
            FrenchRibKey = Regex.Replace(value, @"[^\d]", "").Substring(0, Math.Min(2, value.Length));
        }
    }

    partial void OnDanishRegistrationNumberChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^\d{0,4}$"))
        {
            DanishRegistrationNumber = Regex.Replace(value, @"[^\d]", "").Substring(0, Math.Min(4, value.Length));
        }
    }

    public CompanySettingsViewModel(
        ICompanyRepository companyRepository,
        ILogger logger,
        IBankStatementOcrService ocrService,
        IBankingInfoParser bankingInfoParser)
    {
        _companyRepository = companyRepository;
        _logger = logger;
        _ocrService = ocrService;
        _bankingInfoParser = bankingInfoParser;
        Title = "Company Settings";

        // Subscribe to property changes to track unsaved changes
        PropertyChanged += (sender, e) =>
        {
            // Mark as changed for data properties (not UI state properties)
            if (e.PropertyName != nameof(IsLoading) &&
                e.PropertyName != nameof(HasUnsavedChanges) &&
                e.PropertyName != nameof(ShowFrenchFields) &&
                e.PropertyName != nameof(ShowDanishFields) &&
                e.PropertyName != nameof(ShowFrenchBanking) &&
                e.PropertyName != nameof(ShowDanishBanking) &&
                e.PropertyName != nameof(CanScanStatement) &&
                e.PropertyName != nameof(IsProcessingStatement) &&
                e.PropertyName != nameof(ProcessingStatus))
            {
                MarkAsChanged();
            }
        };
    }

    [RelayCommand]
    public async Task LoadCompanyAsync()
    {
        try
        {
            IsLoading = true;
            _logger.Information("Loading company settings...");

            // Get the first (and likely only) company record
            _currentCompany = await _companyRepository.GetFirstAsync();

            if (_currentCompany != null)
            {
                LoadCompanyData(_currentCompany);
                _logger.Information("Company settings loaded: {CompanyName}", _currentCompany.Name);
            }
            else
            {
                _logger.Information("No company found, using defaults");
                SetDefaults();
            }

            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load company settings");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SaveCompanyAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _logger.Warning("Company name is required");
                MessageBox.Show("Company name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate banking information
            if (!ValidateBankingInformation())
            {
                return;
            }

            IsLoading = true;
            _logger.Information("Saving company settings...");

            // Always get a fresh tracked entity for updates
            var trackedCompany = await _companyRepository.GetFirstForUpdateAsync();

            if (trackedCompany == null)
            {
                // Create new company if none exists
                _logger.Information("Creating new company");
                trackedCompany = new Company(Name, Country);
                await _companyRepository.AddAsync(trackedCompany);
            }
            else
            {
                _logger.Information("Updating existing company");
            }

            // Update the tracked entity with form data
            UpdateCompanyData(trackedCompany);

            // Save changes
            await _companyRepository.SaveChangesAsync();

            // Update our local reference
            _currentCompany = trackedCompany;

            HasUnsavedChanges = false;
            _logger.Information("Company settings saved successfully");

            MessageBox.Show("Company settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save company settings");
            MessageBox.Show($"Failed to save company settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task UploadLogoAsync()
    {
        try
        {
            _logger.Information("Opening logo upload dialog");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Select Company Logo",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                _logger.Information("Logo file selected: {FilePath}", selectedFilePath);

                string logoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logos");
                Directory.CreateDirectory(logoDirectory);

                string fileName = $"company_logo_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(selectedFilePath)}";
                string destinationPath = Path.Combine(logoDirectory, fileName);

                File.Copy(selectedFilePath, destinationPath, true);

                LogoPath = destinationPath;
                LogoPreviewPath = destinationPath;
                HasLogo = true;

                _logger.Information("Logo uploaded successfully: {DestinationPath}", destinationPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upload logo");
        }
    }

    [RelayCommand]
    public void RemoveLogo()
    {
        try
        {
            _logger.Information("Removing company logo");

            LogoPath = string.Empty;
            LogoPreviewPath = string.Empty;
            HasLogo = false;

            _logger.Information("Logo removed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to remove logo");
        }
    }

    [RelayCommand]
    public async Task SelectStatementFileAsync()
    {
        try
        {
            _logger.Information("Opening bank statement file dialog");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Select Bank Statement",
                Filter = "All supported files (*.pdf;*.png;*.jpg;*.jpeg)|*.pdf;*.png;*.jpg;*.jpeg|PDF files (*.pdf)|*.pdf|Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                StatementFilePath = openFileDialog.FileName;
                StatementFileName = Path.GetFileName(StatementFilePath);
                HasStatementFile = true;

                _logger.Information("Bank statement file selected: {FileName}", StatementFileName);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to select bank statement file");
        }
    }

    [RelayCommand]
    public async Task ScanStatementAsync()
    {
        if (!HasStatementFile || string.IsNullOrEmpty(StatementFilePath))
        {
            _logger.Warning("No bank statement file selected for scanning");
            return;
        }

        try
        {
            IsProcessingStatement = true;
            _logger.Information("Starting bank statement scan: {FilePath}", StatementFilePath);

            if (!_ocrService.IsSupportedFormat(StatementFilePath))
            {
                ProcessingStatus = "Unsupported file format";
                _logger.Warning("Unsupported file format: {FilePath}", StatementFilePath);

                MessageBox.Show(
                    $"Unsupported file format: {Path.GetExtension(StatementFilePath)}\n\nSupported formats: PDF, PNG, JPG, JPEG",
                    "File Format Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                await Task.Delay(2000);
                return;
            }

            string extractedText = await _ocrService.ExtractTextAsync(
                StatementFilePath,
                status => ProcessingStatus = status
            );

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                ProcessingStatus = "No text could be extracted";
                _logger.Warning("No text extracted from file: {FilePath}", StatementFilePath);

                MessageBox.Show(
                    "No text could be extracted from the document.\n\nPlease ensure the document is clear and readable.",
                    "OCR Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                await Task.Delay(2000);
                return;
            }

            _logger.Information("OCR extraction successful. Text length: {Length} characters", extractedText.Length);

            ProcessingStatus = "Parsing banking information...";
            await Task.Delay(500);

            var bankingInfo = _bankingInfoParser.ParseBankingInfo(extractedText);

            var resultsMessage = BuildResultsMessage(bankingInfo, extractedText.Length);

            if (bankingInfo.ConfidenceScore > 0.3f)
            {
                ProcessingStatus = "Auto-filling banking fields...";
                await Task.Delay(500);

                // Auto-fill universal banking information fields
                if (!string.IsNullOrEmpty(bankingInfo.BankName))
                {
                    BankName = bankingInfo.BankName;
                    _logger.Information("Auto-filled Bank Name: {BankName}", bankingInfo.BankName);
                }

                if (!string.IsNullOrEmpty(bankingInfo.Iban))
                {
                    Iban = bankingInfo.Iban;
                    _logger.Information("Auto-filled IBAN: {IBAN}", bankingInfo.Iban);
                }

                if (!string.IsNullOrEmpty(bankingInfo.Swift))
                {
                    Swift = bankingInfo.Swift;
                    _logger.Information("Auto-filled SWIFT/BIC: {SWIFT}", bankingInfo.Swift);
                }

                // Auto-fill country-specific banking fields
                if (Country == "FR" && bankingInfo.AdditionalInfo.ContainsKey("FrenchRIB"))
                {
                    var ribInfo = bankingInfo.AdditionalInfo["FrenchRIB"] as dynamic;
                    if (ribInfo != null)
                    {
                        FrenchBankCode = ribInfo.BankCode?.ToString() ?? string.Empty;
                        FrenchBranchCode = ribInfo.BranchCode?.ToString() ?? string.Empty;
                        FrenchAccountNumber = ribInfo.AccountNumber?.ToString() ?? string.Empty;
                        FrenchRibKey = ribInfo.Key?.ToString() ?? string.Empty;

                        _logger.Information("Auto-filled French RIB: {BankCode} {BranchCode} {AccountNumber} {Key}",
                            FrenchBankCode, FrenchBranchCode, FrenchAccountNumber, FrenchRibKey);
                    }
                }
                else if (Country == "DK" && bankingInfo.AdditionalInfo.ContainsKey("DanishBanking"))
                {
                    var danishInfo = bankingInfo.AdditionalInfo["DanishBanking"] as dynamic;
                    if (danishInfo != null)
                    {
                        DanishRegistrationNumber = danishInfo.RegistrationNumber?.ToString() ?? string.Empty;
                        DanishAccountNumber = danishInfo.AccountNumber?.ToString() ?? string.Empty;

                        _logger.Information("Auto-filled Danish Banking: {RegNum} {AccountNum}",
                            DanishRegistrationNumber, DanishAccountNumber);
                    }
                }

                MessageBox.Show(
                    resultsMessage,
                    "Banking Information Extracted Successfully!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ProcessingStatus = $"Banking information extracted successfully! (Confidence: {bankingInfo.ConfidenceScore:P0})";
                await Task.Delay(2000);

                _logger.Information("Banking information auto-fill completed with {Confidence:F2}% confidence",
                    bankingInfo.ConfidenceScore * 100);
            }
            else
            {
                MessageBox.Show(
                    $"Banking information extraction had low confidence ({bankingInfo.ConfidenceScore:P0}).\n\n{resultsMessage}\n\nPlease verify and correct the information manually.",
                    "Low Confidence Extraction",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ProcessingStatus = "Banking information not found or confidence too low";
                _logger.Warning("Banking information parsing confidence too low: {Confidence:F2}%",
                    bankingInfo.ConfidenceScore * 100);
                await Task.Delay(2000);
            }

            _logger.Information("Bank statement scanning completed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to scan bank statement");
            ProcessingStatus = "Scanning failed - check logs";

            MessageBox.Show(
                $"Failed to scan bank statement.\n\nError: {ex.Message}\n\nPlease check the application logs for more details.",
                "Scanning Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            await Task.Delay(2000);
        }
        finally
        {
            IsProcessingStatement = false;
            ProcessingStatus = string.Empty;
        }
    }

    [RelayCommand]
    public void ClearStatement()
    {
        try
        {
            _logger.Information("Clearing bank statement file");

            StatementFilePath = string.Empty;
            StatementFileName = string.Empty;
            HasStatementFile = false;
            ProcessingStatus = string.Empty;

            _logger.Information("Bank statement file cleared");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to clear bank statement");
        }
    }

    private bool ValidateBankingInformation()
    {
        var errors = new List<string>();

        if (Country == "FR")
        {
            if (!string.IsNullOrEmpty(FrenchBankCode) && FrenchBankCode.Length != 5)
                errors.Add("French Bank Code must be exactly 5 digits");

            if (!string.IsNullOrEmpty(FrenchBranchCode) && FrenchBranchCode.Length != 5)
                errors.Add("French Branch Code must be exactly 5 digits");

            if (!string.IsNullOrEmpty(FrenchAccountNumber) && FrenchAccountNumber.Length != 11)
                errors.Add("French Account Number must be exactly 11 alphanumeric characters");

            if (!string.IsNullOrEmpty(FrenchRibKey) && FrenchRibKey.Length != 2)
                errors.Add("French RIB Key must be exactly 2 digits");
        }
        else if (Country == "DK")
        {
            if (!string.IsNullOrEmpty(DanishRegistrationNumber) && DanishRegistrationNumber.Length != 4)
                errors.Add("Danish Registration Number must be exactly 4 digits");
        }

        if (!string.IsNullOrEmpty(Iban) && !IsValidIban(Iban))
            errors.Add("IBAN format is invalid");

        if (errors.Any())
        {
            MessageBox.Show(
                $"Please correct the following banking information errors:\n\n{string.Join("\n", errors)}",
                "Banking Validation Errors",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private bool IsValidIban(string iban)
    {
        var cleanIban = iban.Replace(" ", "").ToUpper();
        return Regex.IsMatch(cleanIban, @"^[A-Z]{2}[0-9]{2}[A-Z0-9]{4,30}$");
    }

    private void LoadCompanyData(Company company)
    {
        // Basic Information
        Name = company.Name ?? string.Empty;
        LegalName = company.LegalName ?? string.Empty;
        Email = company.Email ?? string.Empty;
        Phone = company.Phone ?? string.Empty;
        Website = company.Website ?? string.Empty;

        // Address
        Street = company.Street ?? string.Empty;
        City = company.City ?? string.Empty;
        PostalCode = company.PostalCode ?? string.Empty;
        CountryName = company.CountryName ?? string.Empty;

        // Business Settings
        Country = company.Country ?? "FR";
        LegalForm = company.LegalForm ?? string.Empty;
        BusinessRegistrationNumber = company.BusinessRegistrationNumber ?? string.Empty;
        TaxId = company.TaxId ?? string.Empty;
        VatNumber = company.VatNumber ?? string.Empty;
        IsVatExempt = company.IsVatExempt;

        // French-specific
        Siret = company.Siret ?? string.Empty;
        Siren = company.Siren ?? string.Empty;
        ApeCode = company.ApeCode ?? string.Empty;
        RcsNumber = company.RcsNumber ?? string.Empty;
        FrenchVatExemptionMention = company.FrenchVatExemptionMention ?? "TVA non applicable, art. 293 B du CGI";

        // Danish-specific
        CvrNumber = company.CvrNumber ?? string.Empty;
        DanishVatNumber = company.DanishVatNumber ?? string.Empty;
        SeNumber = company.SENumber ?? string.Empty;
        IntraCommunityVat = company.IntraCommunityVat ?? string.Empty;

        // Universal Banking Information
        BankName = company.BankName ?? string.Empty;
        Iban = company.Iban ?? string.Empty;
        Swift = company.Swift ?? string.Empty;

        // French Banking Information
        FrenchBankCode = company.FrenchBankCode ?? string.Empty;
        FrenchBranchCode = company.FrenchBranchCode ?? string.Empty;
        FrenchAccountNumber = company.FrenchAccountNumber ?? string.Empty;
        FrenchRibKey = company.FrenchRibKey ?? string.Empty;

        // Danish Banking Information
        DanishRegistrationNumber = company.DanishRegistrationNumber ?? string.Empty;
        DanishAccountNumber = company.DanishAccountNumber ?? string.Empty;

        // Insurance Information
        InsuranceCompany = company.InsuranceCompany ?? string.Empty;
        InsurancePolicyNumber = company.InsurancePolicyNumber ?? string.Empty;

        // Branding
        LogoPath = company.LogoPath ?? string.Empty;
        PrimaryColor = company.PrimaryColor ?? "#2563EB";

        // Update logo preview
        if (!string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath))
        {
            LogoPreviewPath = LogoPath;
            HasLogo = true;
        }
        else
        {
            LogoPreviewPath = string.Empty;
            HasLogo = false;
        }

        // Default settings
        DefaultCurrency = company.DefaultCurrency ?? "EUR";
        DefaultTaxRate = company.DefaultTaxRate;
        InvoicePrefix = company.InvoicePrefix ?? "INV";
        QuotePrefix = company.QuotePrefix ?? "QUO";

        // Set country index for UI
        SelectedCountryIndex = company.Country == "DK" ? 1 : 0;
    }

    private void UpdateCompanyData(Company company)
    {
        // Update basic details
        company.UpdateDetails(
            name: Name,
            legalName: LegalName,
            taxId: TaxId,
            vatNumber: VatNumber
        );

        // Update address
        company.UpdateAddress(
            street: Street,
            city: City,
            postalCode: PostalCode,
            country: CountryName
        );

        // Update contact information
        company.UpdateContact(
            email: Email,
            phone: Phone,
            website: Website
        );

        // Update banking information based on country
        if (Country == "FR")
        {
            company.UpdateFrenchBanking(
                bankName: BankName,
                iban: Iban,
                swift: Swift,
                bankCode: FrenchBankCode,
                branchCode: FrenchBranchCode,
                accountNumber: FrenchAccountNumber,
                ribKey: FrenchRibKey
            );
        }
        else if (Country == "DK")
        {
            company.UpdateDanishBanking(
                bankName: BankName,
                iban: Iban,
                swift: Swift,
                registrationNumber: DanishRegistrationNumber,
                accountNumber: DanishAccountNumber
            );
        }
        else
        {
            company.UpdateBanking(
                bankName: BankName,
                iban: Iban,
                swift: Swift
            );
        }

        // Update insurance information
        company.UpdateInsurance(
            insuranceCompany: InsuranceCompany,
            policyNumber: InsurancePolicyNumber
        );

        // Update branding information
        company.UpdateBranding(
            logoPath: LogoPath,
            primaryColor: PrimaryColor
        );

        // Update country-specific registration
        if (Country == "FR")
        {
            company.UpdateFrenchRegistration(
                siret: Siret,
                apeCode: ApeCode,
                  rcsNumber: RcsNumber,
                isVatExempt: IsVatExempt
            );
        }
        else if (Country == "DK")
        {
            company.UpdateDanishRegistration(
                cvrNumber: CvrNumber,
                seNumber: SeNumber,
                isVatExempt: IsVatExempt
            );
        }

        // Set country if changed
        if (company.Country != Country)
        {
            company.SetCountry(Country);
        }
    }

    private void SetDefaults()
    {
        Name = "My Company";
        Country = "FR";
        LegalForm = "Auto-Entrepreneur";
        DefaultCurrency = "EUR";
        DefaultTaxRate = 20.0m;
        FrenchVatExemptionMention = "TVA non applicable, art. 293 B du CGI";
        IsVatExempt = true;
        SelectedCountryIndex = 0;
        InvoicePrefix = "INV";
        QuotePrefix = "QUO";
        PrimaryColor = "#2563EB";
    }

    private void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }

    private string BuildResultsMessage(BankingInfoResult bankingInfo, int textLength)
    {
        var message = new StringBuilder();
        message.AppendLine($"OCR Extraction: {textLength} characters extracted");
        message.AppendLine($"Confidence Score: {bankingInfo.ConfidenceScore:P0}");
        message.AppendLine();

        message.AppendLine("EXTRACTED INFORMATION:");
        message.AppendLine("═══════════════════════");

        if (!string.IsNullOrEmpty(bankingInfo.BankName))
        {
            message.AppendLine($"Bank Name: {bankingInfo.BankName}");
        }
        else
        {
            message.AppendLine("Bank Name: Not detected");
        }

        if (!string.IsNullOrEmpty(bankingInfo.Iban))
        {
            message.AppendLine($"IBAN: {bankingInfo.Iban}");
        }
        else
        {
            message.AppendLine("IBAN: Not detected");
        }

        if (!string.IsNullOrEmpty(bankingInfo.Swift))
        {
            message.AppendLine($"SWIFT/BIC: {bankingInfo.Swift}");
        }
        else
        {
            message.AppendLine("SWIFT/BIC: Not detected");
        }

        // Show French RIB details if available
        if (Country == "FR" && bankingInfo.AdditionalInfo.ContainsKey("FrenchRIB"))
        {
            var ribInfo = bankingInfo.AdditionalInfo["FrenchRIB"] as dynamic;
            if (ribInfo != null)
            {
                message.AppendLine();
                message.AppendLine("FRENCH RIB DETAILS:");
                message.AppendLine($"   Code Banque: {ribInfo.BankCode}");
                message.AppendLine($"   Code Guichet: {ribInfo.BranchCode}");
                message.AppendLine($"   Numéro de Compte: {ribInfo.AccountNumber}");
                message.AppendLine($"   Clé RIB: {ribInfo.Key}");
            }
        }

        // Show Danish banking details if available
        if (Country == "DK" && bankingInfo.AdditionalInfo.ContainsKey("DanishBanking"))
        {
            var danishInfo = bankingInfo.AdditionalInfo["DanishBanking"] as dynamic;
            if (danishInfo != null)
            {
                message.AppendLine();
                message.AppendLine("DANISH BANKING DETAILS:");
                message.AppendLine($"   Registration Number: {danishInfo.RegistrationNumber}");
                message.AppendLine($"   Account Number: {danishInfo.AccountNumber}");
            }
        }

        message.AppendLine();
        if (bankingInfo.ConfidenceScore > 0.7f)
        {
            message.AppendLine("High confidence - Information auto-filled");
        }
        else if (bankingInfo.ConfidenceScore > 0.3f)
        {
            message.AppendLine("Medium confidence - Please verify information");
        }
        else
        {
            message.AppendLine("Low confidence - Manual entry recommended");
        }

        return message.ToString();
    }
}