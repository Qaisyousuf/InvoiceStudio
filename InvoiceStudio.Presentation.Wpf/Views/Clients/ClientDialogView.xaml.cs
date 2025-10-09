using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf.Views.Clients;

public partial class ClientDialogView : Window
{
    public ClientDialogView(ClientDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientDialogViewModel vm)
        {
            bool success = await vm.SaveAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}