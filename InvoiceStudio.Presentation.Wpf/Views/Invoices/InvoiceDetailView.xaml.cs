using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf.Views.Invoices;

public partial class InvoiceDetailView : Window
{
    public InvoiceDetailView(InvoiceDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public static async Task<bool?> ShowAsync(InvoiceDetailViewModel viewModel, Guid invoiceId, Window? owner = null)
    {
        var dialog = new InvoiceDetailView(viewModel);

        if (owner != null)
        {
            dialog.Owner = owner;
        }
        else
        {
            dialog.Owner = System.Windows.Application.Current.MainWindow;
        }

        // Load the invoice data before showing the dialog
        await viewModel.LoadInvoiceAsync(invoiceId);

        return dialog.ShowDialog();
    }
}