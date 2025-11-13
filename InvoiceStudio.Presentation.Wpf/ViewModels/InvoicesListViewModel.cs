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
    private readonly ICompanyRepository _companyRepository;
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

    [ObservableProperty]
    private int _issuedInvoices;

    [ObservableProperty]
    private int _overdueInvoices;

    [ObservableProperty]
    private decimal _outstandingAmount;

    public InvoicesListViewModel(
        IInvoiceRepository invoiceRepository,
        ILogger logger,
        IServiceProvider serviceProvider,
        ICompanyRepository companyRepository)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _companyRepository = companyRepository;
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
        IssuedInvoices = Invoices.Count(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Sent);
        OverdueInvoices = Invoices.Count(i => i.Status == InvoiceStatus.Overdue ||
            (i.Status == InvoiceStatus.Sent && DateTime.UtcNow > i.DueDate));

        TotalRevenue = Invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
        OutstandingAmount = Invoices.Where(i => i.Status != InvoiceStatus.Paid &&
            i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Draft)
            .Sum(i => i.GetOutstandingBalance());
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

            // Call domain method on tracked entity - pay full outstanding amount
            var outstandingAmount = trackedInvoice.GetOutstandingBalance();
            trackedInvoice.MarkAsPaid(outstandingAmount, DateTime.Today);

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
            // Only allow editing of Draft invoices
            if (invoice?.Status != InvoiceStatus.Draft)
            {
                _logger.Warning("Only draft invoices can be edited. Current status: {Status}", invoice?.Status);

                // Show user-friendly message
                System.Windows.MessageBox.Show(
                    "Only draft invoices can be edited.",
                    "Cannot Edit Invoice",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            _logger.Information("Opening edit dialog for invoice {InvoiceNumber}", invoice.InvoiceNumber);

            // Get the InvoiceDialogViewModel from DI (reuse the same dialog for create/edit)
            var dialogViewModel = _serviceProvider.GetRequiredService<InvoiceDialogViewModel>();

            // Load the invoice for editing
            await dialogViewModel.LoadInvoiceForEditAsync(invoice.Id);

            // Create and show the dialog
            var dialog = new InvoiceDialogView(dialogViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show dialog
            var result = dialog.ShowDialog();

            if (result == true)
            {
                _logger.Information("Invoice {InvoiceNumber} edited successfully, refreshing list", invoice.InvoiceNumber);

                // Refresh the invoices list to show updated data
                await LoadInvoicesAsync();
            }
            else
            {
                _logger.Information("Invoice edit cancelled for {InvoiceNumber}", invoice.InvoiceNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening edit dialog for invoice {InvoiceNumber}", invoice?.InvoiceNumber);

            // Show error to user
            System.Windows.MessageBox.Show(
                "An error occurred while opening the invoice for editing. Please try again.",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadInvoicesAsync();
    }

    [RelayCommand]
    private async Task DeleteInvoiceAsync(Invoice invoice)
    {
        try
        {
            if (invoice == null)
            {
                _logger.Warning("Cannot delete null invoice");
                return;
            }

            if (!invoice.CanBeDeleted())
            {
                _logger.Warning("Invoice {InvoiceNumber} cannot be deleted in status {Status}",
                    invoice.InvoiceNumber, invoice.Status);
                return;
            }

            _logger.Information("Deleting invoice {InvoiceNumber}", invoice.InvoiceNumber);

            // Get the tracked entity from database first
            var trackedInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (trackedInvoice == null)
            {
                _logger.Error("Invoice not found: {InvoiceId}", invoice.Id);
                return;
            }

            await _invoiceRepository.DeleteAsync(trackedInvoice);
            await _invoiceRepository.SaveChangesAsync();

            // Remove from local collection
            Invoices.Remove(invoice);
            UpdateDashboardStats();

            _logger.Information("Invoice {InvoiceNumber} deleted successfully", invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete invoice {InvoiceNumber}", invoice?.InvoiceNumber);
        }
    }

    public async Task InitializeAsync()
    {
        await LoadInvoicesAsync();
    }
}