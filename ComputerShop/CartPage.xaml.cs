using System;
using System.Windows;
using System.Windows.Controls;

namespace ComputerShop
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            decimal total = 0;
            foreach (var it in Session.Cart) total += it.Sum;
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = Session.Cart;
            tbTotal.Text = "Итого: " + total.ToString("N2") + " руб.";
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var it = lvCart.SelectedItem as CartItem;
            if (it == null)
            {
                MessageBox.Show("Выберите позицию.");
                return;
            }
            Session.Cart.Remove(it);
            Refresh();
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Session.Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }
            try
            {
                int customerId = Db.GetOrCreateCustomer(Session.CurrentUser);
                Db.PlaceOrder(customerId, Session.Cart);
                Session.Cart.Clear();
                MessageBox.Show("Заказ оформлен!");
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления заказа: " + ex.Message);
            }
        }
    }
}
