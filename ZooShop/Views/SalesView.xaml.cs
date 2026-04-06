using System.Windows.Controls;
using System.Windows.Input;
using ZooShop.ViewModels;

namespace ZooShop.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }

    private void ProductGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SalesViewModel vm && vm.SelectedAvailableProduct != null)
        {
            vm.AddProductToCartCommand.Execute(null);
        }
    }
}
