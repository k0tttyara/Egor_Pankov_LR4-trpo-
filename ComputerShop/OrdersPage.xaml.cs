using System;
using System.Windows;
using System.Windows.Controls;

namespace ComputerShop
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            btnCancel.Visibility = Session.IsClient ? Visibility.Visible : Visibility.Collapsed;
            btnStatus.Visibility = (Session.IsManager || Session.IsAdmin) ? Visibility.Visible : Visibility.Collapsed;
            btnDelete.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                int? forCustomer = Session.IsClient ? (int?)Db.GetOrCreateCustomer(Session.CurrentUser) : null;
                lvOrders.ItemsSource = Db.GetOrders(forCustomer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        private void BtnItems_Click(object sender, RoutedEventArgs e)
        {
            var o = lvOrders.SelectedItem as Order;
            if (o == null)
            {
                MessageBox.Show("Выберите заказ.");
                return;
            }
            new OrderItemsWindow(o.OrderID).ShowDialog();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var o = lvOrders.SelectedItem as Order;
            if (o == null)
            {
                MessageBox.Show("Выберите заказ.");
                return;
            }
            if (o.Status != "Новый")
            {
                MessageBox.Show("Отменить можно только заказ со статусом «Новый».");
                return;
            }
            if (MessageBox.Show("Отменить заказ №" + o.OrderID + "?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                Db.UpdateOrderStatus(o.OrderID, "Отменён");
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnStatus_Click(object sender, RoutedEventArgs e)
        {
            var o = lvOrders.SelectedItem as Order;
            if (o == null)
            {
                MessageBox.Show("Выберите заказ.");
                return;
            }
            var win = new ChangeStatusWindow(o.Status);
            if (win.ShowDialog() == true)
            {
                try
                {
                    Db.UpdateOrderStatus(o.OrderID, win.SelectedStatus);
                    Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var o = lvOrders.SelectedItem as Order;
            if (o == null)
            {
                MessageBox.Show("Выберите заказ.");
                return;
            }
            if (MessageBox.Show("Удалить заказ №" + o.OrderID + "?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                Db.DeleteOrder(o.OrderID);
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
