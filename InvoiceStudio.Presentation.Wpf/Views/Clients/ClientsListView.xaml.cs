using InvoiceStudio.Presentation.Wpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Clients;

public partial class ClientsListView : UserControl
{
    public ClientsListView(ClientsListViewModel viewmdoel)
    {
        InitializeComponent();
        DataContext = viewmdoel;
    }
}