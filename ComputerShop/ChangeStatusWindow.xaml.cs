using System.Collections.Generic;
using System.Windows;

namespace ComputerShop
{
    public partial class ChangeStatusWindow : Window
    {
        public string SelectedStatus;

        public ChangeStatusWindow(string current)
        {
            InitializeComponent();
            cbStatus.ItemsSource = new List<string> { "Новый", "В обработке", "Выполнен", "Отменён" };
            cbStatus.SelectedItem = current;
            if (cbStatus.SelectedIndex < 0) cbStatus.SelectedIndex = 0;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatus = (string)cbStatus.SelectedItem;
            DialogResult = true;
        }
    }
}
