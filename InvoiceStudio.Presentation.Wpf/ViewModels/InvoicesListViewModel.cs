using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class InvoicesListViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger _logger;

    public ObservableCollection<Invoice> Invoices { get; } = new();

    public InvoicesListViewModel(IInvoiceRepository invoiceRepository, ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        Title = "Invoices";
    }

    [RelayCommand]
    private async Task LoadInvoicesAsync()
    {
        try
        {
            IsBusy = true;
            _logger.Information("Starting to load invoices...");

            var invoices = await Task.Run(async () =>
                await _invoiceRepository.GetAllAsync());

            _logger.Information("Retrieved {Count} invoices from database", invoices.Count);

            // Ensure we're on UI thread for ObservableCollection updates
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Invoices.Clear();
                foreach (var invoice in invoices)
                {
                    Invoices.Add(invoice);
                }
                _logger.Information("Successfully loaded {Count} invoices to UI", invoices.Count);
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoices");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CreateInvoice()
    {
        _logger.Information("Create invoice clicked");
        // TODO: Navigate to create invoice view
    }

    [RelayCommand]
    private void ViewInvoice(Invoice invoice)
    {
        if (invoice == null) return;

        _logger.Information("View invoice {InvoiceNumber}", invoice.InvoiceNumber);
        // TODO: Navigate to invoice details view
    }
}