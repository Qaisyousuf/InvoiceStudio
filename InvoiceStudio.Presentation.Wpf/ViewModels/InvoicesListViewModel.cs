using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Invoices;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class InvoicesListViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _isLoading;

    public ObservableCollection<Invoice> Invoices { get; } = new();

    // Dashboard Statistics
    [ObservableProperty]
    private int _totalInvoices;

    [ObservableProperty]
    private int _draftInvoices;

    [ObservableProperty]
    private int _paidInvoices;

    [ObservableProperty]
    private decimal _totalRevenue;

    public InvoicesListViewModel(IInvoiceRepository invoiceRepository, ILogger logger, IServiceProvider serviceProvider)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        Title = "Invoices";
    }

    [RelayCommand]
    private async Task CreateInvoiceAsync()
    {
        try
        {
            _logger.Information("Create invoice clicked");

            // Get the dialog ViewModel from DI container
            var viewModel = _serviceProvider.GetRequiredService<InvoiceDialogViewModel>();
            await viewModel.LoadDataAsync();

            // Create and show the dialog
            var dialog = new InvoiceDialogView(viewModel);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _logger.Information("Invoice created successfully, refreshing list");
                // Refresh the invoice list after successful creation
                await LoadInvoicesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening create invoice dialog");
        }
    }

  

    [RelayCommand]
    private async Task LoadInvoicesAsync()
    {
        if (_isLoading) return;

        try
        {
            _isLoading = true;
            IsBusy = true;
            _logger.Information("Loading invoices...");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var invoices = await _invoiceRepository.GetAllAsync(cts.Token);

            Invoices.Clear();
            foreach (var invoice in invoices)
            {
                Invoices.Add(invoice);
            }

            _logger.Information("Loaded {Count} invoices", invoices.Count);
            UpdateDashboardStats();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load invoices");
            Invoices.Clear();
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    private void UpdateDashboardStats()
    {
        TotalInvoices = Invoices.Count;
        DraftInvoices = Invoices.Count(i => i.Status == InvoiceStatus.Draft);
        PaidInvoices = Invoices.Count(i => i.Status == InvoiceStatus.Paid);
        TotalRevenue = Invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
    }

    [RelayCommand]
    private async Task ApproveInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice?.Status != InvoiceStatus.Draft)
            {
                _logger.Warning("Only draft invoices can be approved");
                return;
            }

            _logger.Information("Approving invoice {InvoiceNumber}", invoice.InvoiceNumber);

            // Get a fresh tracked entity from database
            var trackedInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (trackedInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoice.Id);
                return;
            }

            // Call domain method on tracked entity
            trackedInvoice.Approve();

            // Save changes
            await _invoiceRepository.SaveChangesAsync();

            // Reload the list to show updated status
            await LoadInvoicesAsync();

            _logger.Information("Invoice {InvoiceNumber} approved successfully", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to approve invoice {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }

    [RelayCommand]
    private async Task IssueInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice?.Status != InvoiceStatus.Approved)
            {
                _logger.Warning("Only approved invoices can be issued");
                return;
            }

            _logger.Information("Issuing invoice {InvoiceNumber}", invoice.InvoiceNumber);

            // Get a fresh tracked entity from database
            var trackedInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (trackedInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoice.Id);
                return;
            }

            // Call domain method on tracked entity
            trackedInvoice.Issue();

            // Save changes
            await _invoiceRepository.SaveChangesAsync();

            // Reload the list to show updated status
            await LoadInvoicesAsync();

            _logger.Information("Invoice {InvoiceNumber} issued successfully", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to issue invoice {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }

    [RelayCommand]
    private async Task MarkAsPaidAsync(Invoice invoice)
    {
        try
        {
            if (invoice == null || invoice.Status == InvoiceStatus.Paid)
            {
                _logger.Warning("Invoice is already paid or invalid");
                return;
            }

            _logger.Information("Marking invoice {InvoiceNumber} as paid", invoice.InvoiceNumber);

            // Get a fresh tracked entity from database
            var trackedInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (trackedInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoice.Id);
                return;
            }

            // Call domain method on tracked entity
            trackedInvoice.MarkAsPaid(trackedInvoice.TotalAmount - trackedInvoice.PaidAmount, DateTime.Today);

            // Save changes
            await _invoiceRepository.SaveChangesAsync();

            // Reload the list to show updated status
            await LoadInvoicesAsync();

            _logger.Information("Invoice {InvoiceNumber} marked as paid", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to mark invoice as paid {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }

    [RelayCommand]
    private async Task CancelInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice?.Status == InvoiceStatus.Paid)
            {
                _logger.Warning("Cannot cancel paid invoices");
                return;
            }

            _logger.Information("Cancelling invoice {InvoiceNumber}", invoice.InvoiceNumber);

            // Get a fresh tracked entity from database
            var trackedInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (trackedInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoice.Id);
                return;
            }

            // Call domain method on tracked entity
            trackedInvoice.Cancel();

            // Save changes
            await _invoiceRepository.SaveChangesAsync();

            // Reload the list to show updated status
            await LoadInvoicesAsync();

            _logger.Information("Invoice {InvoiceNumber} cancelled successfully", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to cancel invoice {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }
    [RelayCommand]
    private async Task ViewInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice == null)
            {
                _logger.Warning("Cannot view null invoice");
                return;
            }

            _logger.Information("Opening invoice detail view for {InvoiceNumber}", invoice.InvoiceNumber);

            // Get the ViewModel from DI container
            var viewModel = _serviceProvider.GetRequiredService<InvoiceDetailViewModel>();

            // Show the detail dialog
            var result = await InvoiceDetailView.ShowAsync(viewModel, invoice.Id, System.Windows.Application.Current.MainWindow);

            _logger.Information("Invoice detail view closed for {InvoiceNumber}", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening invoice detail view for {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }

    [RelayCommand]
    private async Task EditInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice?.Status != InvoiceStatus.Draft)
            {
                _logger.Warning("Only draft invoices can be edited");
                return;
            }

            _logger.Information("Opening edit dialog for invoice {InvoiceNumber}", invoice.InvoiceNumber);

            var viewModel = _serviceProvider.GetRequiredService<EditInvoiceViewModel>();
            await viewModel.LoadInvoiceAsync(invoice.Id);

            var dialog = new EditInvoiceView(viewModel);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _logger.Information("Invoice edited successfully, refreshing list");
                await LoadInvoicesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening edit dialog");
        }
    }
}