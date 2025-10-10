using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Microsoft.VisualBasic;
using Serilog;
using System.Collections.ObjectModel;
using static Azure.Core.HttpHeader;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class InvoiceDetailViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger _logger;

    // Invoice Details
    [ObservableProperty]
    private Invoice? _invoice;

    [ObservableProperty]
    private string _invoiceNumber = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private DateTime _issueDate;

    [ObservableProperty]
    private DateTime _dueDate;

    [ObservableProperty]
    private string _currency = string.Empty;

    // Client Details
    [ObservableProperty]
    private string _clientName = string.Empty;

    [ObservableProperty]
    private string _clientEmail = string.Empty;

    [ObservableProperty]
    private string _clientAddress = string.Empty;

    // Invoice Lines
    public ObservableCollection<InvoiceLine> InvoiceLines { get; } = new();

    // Totals
    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _paidAmount;

    [ObservableProperty]
    private decimal _remainingAmount;

    // Additional Info
    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _terms;

    [ObservableProperty]
    private string? _legalMentions;

    // Status Indicators
    [ObservableProperty]
    private bool _isPaid;

    [ObservableProperty]
    private bool _isOverdue;

    public InvoiceDetailViewModel(IInvoiceRepository invoiceRepository, ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        Title = "Invoice Details";
    }

    public async Task LoadInvoiceAsync(Guid invoiceId)
    {
        try
        {
            IsBusy = true;
            _logger.Information("Loading invoice details for {InvoiceId}", invoiceId);

            // Get invoice with all related data
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
            if (invoice == null)
            {
                _logger.Warning("Invoice not found: {InvoiceId}", invoiceId);
                return;
            }

            // Set invoice data
            Invoice = invoice;
            InvoiceNumber = invoice.InvoiceNumber;
            Status = invoice.Status.ToString();
            IssueDate = invoice.IssueDate;
            DueDate = invoice.DueDate;
            Currency = invoice.Currency;

            // Set client data
            if (invoice.Client != null)
            {
                ClientName = invoice.Client.Name;
                ClientEmail = invoice.Client.Email ?? "No email provided";
                ClientAddress = BuildClientAddress(invoice.Client);
            }

            // Set totals
            SubTotal = invoice.SubTotal;
            TaxAmount = invoice.TaxAmount;
            DiscountAmount = invoice.DiscountAmount;
            TotalAmount = invoice.TotalAmount;
            PaidAmount = invoice.PaidAmount;
            RemainingAmount = invoice.TotalAmount - invoice.PaidAmount;

            // Set additional info
            Notes = invoice.Notes;
            Terms = invoice.Terms;
            LegalMentions = invoice.LegalMentions;

            // Set status indicators
            IsPaid = invoice.Status == InvoiceStatus.Paid;
            IsOverdue = invoice.Status == InvoiceStatus.Overdue;

            // Load invoice lines
            InvoiceLines.Clear();
            foreach (var line in invoice.Lines.OrderBy(l => l.LineOrder))
            {
                InvoiceLines.Add(line);
            }

            // Update title with invoice number
            Title = $"Invoice Details - {invoice.InvoiceNumber}";

            _logger.Information("Successfully loaded invoice details for {InvoiceNumber}", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoice details for {InvoiceId}", invoiceId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string BuildClientAddress(Client client)
    {
        var addressParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(client.Street))
            addressParts.Add(client.Street);

        var cityLine = new List<string>();
        if (!string.IsNullOrWhiteSpace(client.PostalCode))
            cityLine.Add(client.PostalCode);
        if (!string.IsNullOrWhiteSpace(client.City))
            cityLine.Add(client.City);

        if (cityLine.Any())
            addressParts.Add(string.Join(" ", cityLine));

        if (!string.IsNullOrWhiteSpace(client.CountryName))
            addressParts.Add(client.CountryName);

        return addressParts.Any() ? string.Join("\n", addressParts) : "No address provided";
    }

    [RelayCommand]
    private void Close()
    {
        // This will be handled by the dialog itself
        _logger.Information("Closing invoice detail view for {InvoiceNumber}", InvoiceNumber);
    }

    [RelayCommand]
    private void Print()
    {
        // TODO: Implement PDF generation and printing
        _logger.Information("Print requested for invoice {InvoiceNumber}", InvoiceNumber);
    }

    [RelayCommand]
    private void Email()
    {
        // TODO: Implement email sending
        _logger.Information("Email requested for invoice {InvoiceNumber}", InvoiceNumber);
    }
}