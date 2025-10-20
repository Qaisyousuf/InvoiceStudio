using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf.Views.Clients;

public partial class ClientDialogView : Window
{
    private ClientDialogViewModel? _viewModel;

    public ClientDialogView(ClientDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            bool success = await _viewModel.SaveAsync();
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