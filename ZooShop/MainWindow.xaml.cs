using System.Windows;
using ZooShop.ViewModels;

namespace ZooShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }
    }
}