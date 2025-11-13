using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

// Fixed InvoiceLineViewModel - removed conflicting partial methods
public partial class InvoiceLineViewModel : ObservableObject
{
    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _description = string.Empty;

    private decimal _quantity = 1;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            SetProperty(ref _quantity, value);
            UpdateCalculatedProperties();
        }
    }

    private decimal _unitPrice = 0;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            SetProperty(ref _unitPrice, value);
            UpdateCalculatedProperties();
        }
    }

    private decimal _taxRate = 20.0m;
    public decimal TaxRate
    {
        get => _taxRate;
        set
        {
            SetProperty(ref _taxRate, value);
            UpdateCalculatedProperties();
        }
    }

    public decimal LineTotal => Quantity * UnitPrice;
    public decimal TaxAmount => LineTotal * (TaxRate / 100);
    public decimal LineTotalWithTax => LineTotal + TaxAmount;
    public decimal SubTotal => LineTotal;

    private void UpdateCalculatedProperties()
    {
        OnPropertyChanged(nameof(LineTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(LineTotalWithTax));
        OnPropertyChanged(nameof(SubTotal));
    }

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value != null)
        {
            Description = value.Name;
            UnitPrice = value.UnitPrice;
            TaxRate = value.TaxRate;
        }
    }
}

public partial class InvoiceDialogViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger _logger;

    // Invoice Basic Information
    [ObservableProperty]
    private string _invoiceNumber = string.Empty;

    [ObservableProperty]
    private string _currency = "EUR";

    [ObservableProperty]
    private DateTime _issueDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(30);

    // Client Information
    [ObservableProperty]
    private Client? _selectedClient;

    public ObservableCollection<Client> AvailableClients { get; } = new();
    public ObservableCollection<Product> AvailableProducts { get; } = new();

    // Invoice Lines
    public ObservableCollection<InvoiceLineViewModel> InvoiceLines { get; } = new();

    // Additional Information
    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _terms;

    // Company Information Properties (Read-only for display)
    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _companyLegalForm = string.Empty;

    [ObservableProperty]
    private string _companySiret = string.Empty;

    [ObservableProperty]
    private string _companyApeCode = string.Empty;

    [ObservableProperty]
    private string _companyStreet = string.Empty;

    [ObservableProperty]
    private string _companyCity = string.Empty;

    [ObservableProperty]
    private string _companyPostalCode = string.Empty;

    [ObservableProperty]
    private string _companyCountry = string.Empty;

    [ObservableProperty]
    private string _companyEmail = string.Empty;

    [ObservableProperty]
    private string _companyPhone = string.Empty;

    [ObservableProperty]
    private string _companyWebsite = string.Empty;

    [ObservableProperty]
    private string _companyBankName = string.Empty;

    [ObservableProperty]
    private string _companyIban = string.Empty;

    [ObservableProperty]
    private string _companySwift = string.Empty;

    [ObservableProperty]
    private string _companyVatNumber = string.Empty;

    [ObservableProperty]
    private string _companyBusinessRegistration = string.Empty;

    // French RIB Components
    [ObservableProperty]
    private string _companyFrenchRib = string.Empty;

    [ObservableProperty]
    private string _companyFrenchBankCode = string.Empty;

    [ObservableProperty]
    private string _companyFrenchBranchCode = string.Empty;

    [ObservableProperty]
    private string _companyFrenchAccountNumber = string.Empty;

    [ObservableProperty]
    private string _companyFrenchRibKey = string.Empty;

    // Calculated Totals
    public decimal SubTotal => InvoiceLines.Sum(line => line.SubTotal);
    public decimal TaxAmount => InvoiceLines.Sum(line => line.TaxAmount);
    public decimal TotalAmount => SubTotal + TaxAmount;

    // Edit Mode Properties
    private Invoice? _existingInvoice;
    private Company? _currentCompany;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public InvoiceDialogViewModel(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IProductRepository productRepository,
        ICompanyRepository companyRepository,
        ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _companyRepository = companyRepository;
        _logger = logger;
        Title = "New Invoice";

        // Generate invoice number
        GenerateInvoiceNumber();

        // Subscribe to line changes to update totals
        InvoiceLines.CollectionChanged += (s, e) => UpdateTotals();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            _logger.Information("Loading invoice dialog data...");

            // Load available clients
            var clients = await _clientRepository.GetActiveClientsAsync();
            AvailableClients.Clear();
            foreach (var client in clients)
            {
                AvailableClients.Add(client);
            }

            // Load available products
            var products = await _productRepository.GetActiveProductsAsync();
            AvailableProducts.Clear();
            foreach (var product in products)
            {
                AvailableProducts.Add(product);
            }

            // Load company information
            _currentCompany = await _companyRepository.GetFirstAsync();
            if (_currentCompany != null)
            {
                LoadCompanyInfo(_currentCompany);
            }

            _logger.Information("Loaded {ClientCount} clients, {ProductCount} products, and company information",
                clients.Count, products.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoice dialog data");
            HasValidationErrors = true;
            ValidationMessage = "Failed to load required data. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadCompanyInfo(Company company)
    {
        CompanyName = company.Name;
        CompanyLegalForm = company.LegalForm ?? string.Empty;
        CompanySiret = company.Siret ?? string.Empty;
        CompanyApeCode = company.ApeCode ?? string.Empty;
        CompanyStreet = company.Street ?? string.Empty;
        CompanyCity = company.City ?? string.Empty;
        CompanyPostalCode = company.PostalCode ?? string.Empty;
        CompanyCountry = company.CountryName ?? string.Empty; // Use CountryName instead of Country
        CompanyEmail = company.Email ?? string.Empty;
        CompanyPhone = company.Phone ?? string.Empty;
        CompanyWebsite = company.Website ?? string.Empty;
        CompanyBankName = company.BankName ?? string.Empty;
        CompanyIban = company.Iban ?? string.Empty;
        CompanySwift = company.Swift ?? string.Empty;
        CompanyVatNumber = company.VatNumber ?? string.Empty;
        CompanyBusinessRegistration = company.BusinessRegistrationNumber ?? string.Empty;

        // Load French RIB components if available
        CompanyFrenchBankCode = company.FrenchBankCode ?? string.Empty;
        CompanyFrenchBranchCode = company.FrenchBranchCode ?? string.Empty;
        CompanyFrenchAccountNumber = company.FrenchAccountNumber ?? string.Empty;
        CompanyFrenchRibKey = company.FrenchRibKey ?? string.Empty;

        // Format French RIB if components are available
        if (!string.IsNullOrEmpty(company.FrenchBankCode))
        {
            CompanyFrenchRib = $"{company.FrenchBankCode} {company.FrenchBranchCode} {company.FrenchAccountNumber} {company.FrenchRibKey}";
        }

        // Set default currency from company
        Currency = company.DefaultCurrency;
    }

    [RelayCommand]
    private void AddInvoiceLine()
    {
        var newLine = new InvoiceLineViewModel();

        // Subscribe to property changes to update totals
        newLine.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InvoiceLineViewModel.LineTotal) ||
                e.PropertyName == nameof(InvoiceLineViewModel.TaxAmount) ||
                e.PropertyName == nameof(InvoiceLineViewModel.SubTotal))
            {
                UpdateTotals();
            }
        };

        InvoiceLines.Add(newLine);
        _logger.Information("Added new invoice line");
        ClearValidation();
    }

    [RelayCommand]
    private void RemoveInvoiceLine(InvoiceLineViewModel line)
    {
        if (line != null && InvoiceLines.Contains(line))
        {
            InvoiceLines.Remove(line);
            UpdateTotals();
            _logger.Information("Removed invoice line");
        }
    }

    private void UpdateTotals()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }

    private bool ValidateInvoice()
    {
        var errors = new List<string>();

        if (SelectedClient == null)
            errors.Add("Client selection is required");

        if (string.IsNullOrWhiteSpace(InvoiceNumber))
            errors.Add("Invoice number is required");

        if (!InvoiceLines.Any())
            errors.Add("At least one invoice line is required");

        if (InvoiceLines.Any(line => line.Quantity <= 0))
            errors.Add("All invoice lines must have quantity greater than 0");

        if (InvoiceLines.Any(line => line.UnitPrice < 0))
            errors.Add("Unit price cannot be negative");

        if (IssueDate > DueDate)
            errors.Add("Due date must be after issue date");

        if (_currentCompany == null)
            errors.Add("Company information is required");

        HasValidationErrors = errors.Any();
        ValidationMessage = string.Join("; ", errors);

        return !HasValidationErrors;
    }

    private void ClearValidation()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        try
        {
            if (!ValidateInvoice())
            {
                _logger.Warning("Invoice validation failed: {ValidationMessage}", ValidationMessage);
                return false;
            }

            IsBusy = true;

            if (IsEditMode && _existingInvoice != null)
            {
                // UPDATE EXISTING INVOICE
                _logger.Information("Updating existing invoice {InvoiceNumber}", InvoiceNumber);

                var trackedInvoice = await _invoiceRepository.GetByIdWithDetailsAsync(_existingInvoice.Id);
                if (trackedInvoice == null)
                {
                    HasValidationErrors = true;
                    ValidationMessage = "Invoice not found for update";
                    return false;
                }

                // Update dates and notes
                trackedInvoice.UpdateDates(IssueDate, DueDate);
                trackedInvoice.UpdateNotes(Notes, Terms);

                // Clear all existing lines
                var linesToRemove = trackedInvoice.Lines.ToList();
                foreach (var line in linesToRemove)
                {
                    trackedInvoice.RemoveLine(line);
                }

                // Add all lines fresh (this avoids any UpdateDetails calls)
                for (int i = 0; i < InvoiceLines.Count; i++)
                {
                    var lineVm = InvoiceLines[i];

                    var newLine = new InvoiceLine(
                        trackedInvoice.Id,
                        lineVm.Description,
                        lineVm.Quantity,
                        lineVm.UnitPrice,
                        lineVm.TaxRate / 100,
                        lineVm.SelectedProduct?.Id
                    );

                    newLine.SetLineOrder(i + 1);
                    trackedInvoice.AddLine(newLine);
                }

                ApplyCompanyDefaults(trackedInvoice);
                await _invoiceRepository.UpdateAsync(trackedInvoice);
                await _invoiceRepository.SaveChangesAsync();

                _logger.Information("Invoice {InvoiceNumber} updated successfully", InvoiceNumber);
            }
            else
            {
                // CREATE NEW INVOICE
                _logger.Information("Creating new invoice {InvoiceNumber}", InvoiceNumber);

                var invoice = new Invoice(
                    InvoiceNumber,
                    SelectedClient!.Id,
                    _currentCompany!.Id,
                    IssueDate,
                    DueDate,
                    Currency);

                ApplyCompanyDefaults(invoice);

                // Add all lines
                for (int i = 0; i < InvoiceLines.Count; i++)
                {
                    var lineVm = InvoiceLines[i];

                    var invoiceLine = new InvoiceLine(
                        invoice.Id,
                        lineVm.Description,
                        lineVm.Quantity,
                        lineVm.UnitPrice,
                        lineVm.TaxRate,
                        lineVm.SelectedProduct?.Id
                    );

                    invoiceLine.SetLineOrder(i + 1);
                    invoice.AddLine(invoiceLine);
                }

                if (!string.IsNullOrWhiteSpace(Notes) || !string.IsNullOrWhiteSpace(Terms))
                    invoice.UpdateNotes(Notes, Terms);

                await _invoiceRepository.AddAsync(invoice);
                await _invoiceRepository.SaveChangesAsync();

                _logger.Information("Invoice {InvoiceNumber} created successfully", InvoiceNumber);
            }

            ClearValidation();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving invoice: {Message}", ex.Message);
            HasValidationErrors = true;
            ValidationMessage = $"Failed to save invoice. Please try again.";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
    private Invoice CreateNewInvoice()
{
    return new Invoice(
        InvoiceNumber,
        SelectedClient!.Id,
        _currentCompany!.Id,
        IssueDate,
        DueDate,
        Currency);
}

private void AddInvoiceLines(Invoice invoice)
{
    for (int i = 0; i < InvoiceLines.Count; i++)
    {
        var lineVm = InvoiceLines[i];
        
        var invoiceLine = new InvoiceLine(
            invoice.Id,
            lineVm.Description,
            lineVm.Quantity,
            lineVm.UnitPrice,
            ConvertTaxRateToDecimal(lineVm.TaxRate),
            lineVm.SelectedProduct?.Id
        );

        invoiceLine.SetLineOrder(i + 1);
        invoice.AddLine(invoiceLine);
    }
}

private async Task UpdateInvoiceLinesAsync(Invoice trackedInvoice)
{
    // More efficient line update - only modify what changed
    var existingLines = trackedInvoice.Lines.ToList();
    
    // Remove lines that no longer exist
    for (int i = existingLines.Count - 1; i >= InvoiceLines.Count; i--)
    {
        trackedInvoice.RemoveLine(existingLines[i]);
    }

    // Update or add lines
    for (int i = 0; i < InvoiceLines.Count; i++)
    {
        var lineVm = InvoiceLines[i];
        
        if (i < existingLines.Count)
        {
            // Update existing line
            var existingLine = existingLines[i];
            existingLine.UpdateDetails(
                lineVm.Description,
                lineVm.Quantity,
                lineVm.UnitPrice,
                ConvertTaxRateToDecimal(lineVm.TaxRate),
                lineVm.SelectedProduct?.Id
            );
            existingLine.SetLineOrder(i + 1);
        }
        else
        {
            // Add new line
            var newLine = new InvoiceLine(
                trackedInvoice.Id,
                lineVm.Description,
                lineVm.Quantity,
                lineVm.UnitPrice,
                ConvertTaxRateToDecimal(lineVm.TaxRate),
                lineVm.SelectedProduct?.Id
            );
            newLine.SetLineOrder(i + 1);
            trackedInvoice.AddLine(newLine);
        }
    }
}

private decimal ConvertTaxRateToDecimal(decimal taxRatePercentage)
{
    return taxRatePercentage / 100; // 20.0 -> 0.20
}

    private void ApplyCompanyDefaults(Invoice invoice)
    {
        if (_currentCompany == null) return;

        // Set payment terms (use a default since your Company entity doesn't have DefaultPaymentTerms)
        invoice.SetPaymentTerms("Paiement à réception");

        // Set legal mentions based on company country
        if (_currentCompany.Country?.ToUpper() == "FR")
        {
            invoice.SetFrenchLegalMentions(
                _currentCompany.Siret ?? string.Empty,
                _currentCompany.ApeCode ?? string.Empty,
                _currentCompany.IsVatExempt
            );
        }
        else if (_currentCompany.Country?.ToUpper() == "DK")
        {
            invoice.SetDanishLegalMentions(
                _currentCompany.CvrNumber ?? string.Empty,
                _currentCompany.IsVatExempt
            );
        }
    }

    [RelayCommand]
    private void CancelDialog()
    {
        _logger.Information("Invoice creation cancelled");
        ClearValidation();
    }

    [RelayCommand]
    private void ClearForm()
    {
        InvoiceLines.Clear();
        SelectedClient = null;
        Notes = null;
        Terms = null;
        IssueDate = DateTime.Today;
        DueDate = DateTime.Today.AddDays(30);
        GenerateInvoiceNumber();
        ClearValidation();
        _logger.Information("Invoice form cleared");
    }

    private void GenerateInvoiceNumber()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        InvoiceNumber = $"INV-{timestamp}";
    }

    // For edit mode functionality
    public async Task LoadInvoiceForEditAsync(Guid invoiceId)
    {
        try
        {
            IsBusy = true;
            IsEditMode = true;
            Title = "Edit Invoice";

            _existingInvoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (_existingInvoice == null)
            {
                HasValidationErrors = true;
                ValidationMessage = "Invoice not found";
                return;
            }

            // Load basic data first
            await LoadDataAsync();

            // Populate form with existing invoice data
            InvoiceNumber = _existingInvoice.InvoiceNumber;
            Currency = _existingInvoice.Currency;
            IssueDate = _existingInvoice.IssueDate;
            DueDate = _existingInvoice.DueDate;
            SelectedClient = AvailableClients.FirstOrDefault(c => c.Id == _existingInvoice.ClientId);
            Notes = _existingInvoice.Notes;
            Terms = _existingInvoice.Terms;

            // Load existing lines
            InvoiceLines.Clear();
            foreach (var line in _existingInvoice.Lines)
            {
                var lineVm = new InvoiceLineViewModel
                {
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxRate = line.TaxRate,
                    SelectedProduct = AvailableProducts.FirstOrDefault(p => p.Id == line.ProductId)
                };

                // Subscribe to changes
                lineVm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(InvoiceLineViewModel.LineTotal) ||
                        e.PropertyName == nameof(InvoiceLineViewModel.TaxAmount) ||
                        e.PropertyName == nameof(InvoiceLineViewModel.SubTotal))
                    {
                        UpdateTotals();
                    }
                };

                InvoiceLines.Add(lineVm);
            }

            UpdateTotals();
            _logger.Information("Loaded invoice {InvoiceNumber} for editing", _existingInvoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoice for edit");
            HasValidationErrors = true;
            ValidationMessage = "Failed to load invoice for editing";
        }
        finally
        {
            IsBusy = false;
        }
    }
    public async Task LoadInvoiceForEditingAsync(Guid invoiceId)
    {
        try
        {
            IsBusy = true;
            IsEditMode = true;
            _logger.Information("Loading invoice {InvoiceId} for editing", invoiceId);

            // Load the existing invoice with all its data
            var existingInvoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
            if (existingInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoiceId);
                return;
            }

            _existingInvoice = existingInvoice;

            // Populate the form fields with existing invoice data
            InvoiceNumber = existingInvoice.InvoiceNumber ?? string.Empty;
            Currency = existingInvoice.Currency ?? "EUR";
            IssueDate = existingInvoice.IssueDate;
            DueDate = existingInvoice.DueDate;
            Notes = existingInvoice.Notes ?? string.Empty;
            Terms = existingInvoice.Terms;

            // Set the selected client
            SelectedClient = AvailableClients.FirstOrDefault(c => c.Id == existingInvoice.ClientId);

            // Load existing invoice lines
            InvoiceLines.Clear();
            foreach (var line in existingInvoice.Lines)
            {
                var lineViewModel = new InvoiceLineViewModel
                {
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxRate = line.TaxRate,
                    SelectedProduct = AvailableProducts.FirstOrDefault(p => p.Id == line.ProductId)
                };
                InvoiceLines.Add(lineViewModel);
            }

            // Update totals
            UpdateTotals();

            // Change dialog title
            Title = $"Edit Invoice - {existingInvoice.InvoiceNumber}";

            _logger.Information("Invoice {InvoiceNumber} loaded for editing successfully", existingInvoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load invoice for editing");
            HasValidationErrors = true;
            ValidationMessage = "Failed to load invoice for editing. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}