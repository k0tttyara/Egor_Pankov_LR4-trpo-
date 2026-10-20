using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ComputerShop
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
            btnAdd.Visibility = Session.IsClient ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadCategories()
        {
            try
            {
                var list = new List<string> { "Все категории" };
                foreach (var c in Db.GetCategories()) list.Add(c.Name);
                cbCategory.ItemsSource = list;
                cbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void LoadProducts()
        {
            try
            {
                int? catId = null;
                string catName = (string)cbCategory.SelectedItem;
                if (!string.IsNullOrEmpty(catName) && catName != "Все категории")
                    foreach (var c in Db.GetCategories())
                        if (c.Name == catName) catId = c.CategoryID;

                lvProducts.ItemsSource = Db.GetProducts(catId, tbSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var p = lvProducts.SelectedItem as Product;
            if (p == null)
            {
                MessageBox.Show("Выберите товар в списке.");
                return;
            }

            int qty;
            if (!int.TryParse(tbQty.Text, out qty) || qty < 1)
            {
                MessageBox.Show("Количество — целое число не меньше 1.");
                return;
            }
            if (qty > p.Stock)
            {
                MessageBox.Show("На складе только " + p.Stock + " шт.");
                return;
            }

            Session.Cart.Add(new CartItem { Product = p, Quantity = qty });
            MessageBox.Show("Товар добавлен в корзину.");
        }
    }
}
