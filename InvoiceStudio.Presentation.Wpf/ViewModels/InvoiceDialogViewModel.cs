using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

// Add this helper class for line item management
public partial class InvoiceLineViewModel : ObservableObject
{
    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice = 0;

    [ObservableProperty]
    private decimal _taxRate = 20.0m;

   

    public decimal LineTotal => Quantity * UnitPrice;
    public decimal TaxAmount => LineTotal * (TaxRate / 100);
    public decimal LineTotalWithTax => LineTotal + TaxAmount;

    partial void OnQuantityChanged(decimal value) => UpdateCalculatedProperties();
    partial void OnUnitPriceChanged(decimal value) => UpdateCalculatedProperties();
    partial void OnTaxRateChanged(decimal value) => UpdateCalculatedProperties();

    private void UpdateCalculatedProperties()
    {
        OnPropertyChanged(nameof(LineTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(LineTotalWithTax));
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

    // Calculated Totals
    public decimal SubTotal => InvoiceLines.Sum(line => line.LineTotal);
    public decimal TaxAmount => InvoiceLines.Sum(line => line.TaxAmount);
    public decimal TotalAmount => SubTotal + TaxAmount;

    public InvoiceDialogViewModel(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IProductRepository productRepository,
        ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
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

            _logger.Information("Loaded {ClientCount} clients and {ProductCount} products",
                clients.Count, products.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoice dialog data");
        }
    }

    [RelayCommand]
    private void AddInvoiceLine()
    {
        var newLine = new InvoiceLineViewModel();

        // Subscribe to property changes to update totals
        newLine.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InvoiceLineViewModel.LineTotal) ||
                e.PropertyName == nameof(InvoiceLineViewModel.TaxAmount))
            {
                UpdateTotals();
            }
        };

        InvoiceLines.Add(newLine);
        _logger.Information("Added new invoice line");
    }

    [RelayCommand]
    private void RemoveInvoiceLine(InvoiceLineViewModel line)
    {
        if (line != null && InvoiceLines.Contains(line))
        {
            InvoiceLines.Remove(line);
            _logger.Information("Removed invoice line");
        }
    }

    private void UpdateTotals()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }
    [RelayCommand]

    public async Task<bool> SaveAsync()
    {
        try
        {
            if (SelectedClient == null)
            {
                _logger.Warning("Client selection is required");
                return false;
            }
            if (string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                _logger.Warning("Invoice number is required");
                return false;
            }
            if (!InvoiceLines.Any())
            {
                _logger.Warning("At least one invoice line is required");
                return false;
            }

            IsBusy = true;
            _logger.Information("Creating new invoice {InvoiceNumber}", InvoiceNumber);

            // Create new invoice
            var invoice = new Invoice(InvoiceNumber, SelectedClient.Id, IssueDate, DueDate, Currency);

            // Set notes and terms if available
            if (!string.IsNullOrEmpty(Notes) || !string.IsNullOrEmpty(Terms))
            {
                invoice.UpdateNotes(Notes, Terms);
            }

            // Add invoice lines BEFORE saving (let EF Core handle the relationships)
            foreach (var lineVm in InvoiceLines)
            {
                if (lineVm.Quantity > 0 && lineVm.UnitPrice >= 0) // Allow 0 price for free items
                {
                    var invoiceLine = new InvoiceLine(
                        invoice.Id,                    // invoice.Id (will be a new Guid, EF Core will handle FK)
                        lineVm.Description,            // description
                        lineVm.Quantity,               // quantity
                        lineVm.UnitPrice,              // unitPrice
                        lineVm.TaxRate,                // taxRate
                        lineVm.SelectedProduct?.Id     // productId (optional)
                    );
                    invoice.AddLine(invoiceLine);
                }
            }

            // Save everything in one operation
            await _invoiceRepository.AddAsync(invoice);
            await _invoiceRepository.SaveChangesAsync();

            _logger.Information("Invoice {InvoiceNumber} created successfully with {LineCount} lines",
                InvoiceNumber, invoice.Lines.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating invoice");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void GenerateInvoiceNumber()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        InvoiceNumber = $"INV-{timestamp}";
    }

    // Add these new properties to your existing class
    private Invoice? _existingInvoice;

    [ObservableProperty]
    private bool _isEditMode;

    // Add this new method to load existing invoice data
  
}