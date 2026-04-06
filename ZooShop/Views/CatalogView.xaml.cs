using System.Windows.Controls;
using ZooShop.ViewModels;

namespace ZooShop.Views;

public partial class CatalogView : UserControl
{
    public CatalogView()
    {
        InitializeComponent();
    }

    private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CatalogViewModel vm)
        {
            vm.LoadProductStock();
        }
    }
}
