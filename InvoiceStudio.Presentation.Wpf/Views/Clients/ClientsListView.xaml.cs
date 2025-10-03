using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Clients;

public partial class ClientsListView : UserControl
{
    public ClientsListView(ClientsListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}