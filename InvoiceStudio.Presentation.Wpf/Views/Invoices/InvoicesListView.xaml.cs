using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Invoices;

public partial class InvoicesListView : UserControl
{
    public InvoicesListView(InvoicesListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Auto-load when view loads
        Loaded += (s, e) => _ = viewModel.LoadInvoicesCommand.ExecuteAsync(null);
    }
}