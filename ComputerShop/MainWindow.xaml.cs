using System.Windows;

namespace ComputerShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "Магазин компьютерной техники — " + Session.CurrentUser.Login + " (" + Session.CurrentUser.Role + ")";
            miCart.Visibility = Session.IsClient ? Visibility.Visible : Visibility.Collapsed;
            MainFrame.Navigate(new CatalogPage());
        }

        private void MenuCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void MenuCart_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void MenuOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            Session.Cart.Clear();
            new LoginWindow().Show();
            Close();
        }
    }
}
