using System.Windows;

namespace ComputerShop
{
    public partial class OrderItemsWindow : Window
    {
        public OrderItemsWindow(int orderId)
        {
            InitializeComponent();
            lvItems.ItemsSource = Db.GetOrderItems(orderId);
        }
    }
}
