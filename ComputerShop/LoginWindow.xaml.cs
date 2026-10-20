using System;
using System.Windows;

namespace ComputerShop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            try
            {
                Db.EnsureDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе: " + ex.Message);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = tbLogin.Text.Trim();
                if (login.Length == 0 || pbPassword.Password.Length == 0)
                {
                    MessageBox.Show("Введите логин и пароль.");
                    return;
                }

                User user = Db.GetUser(login, Db.Sha256(pbPassword.Password));
                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль.");
                    return;
                }

                Session.CurrentUser = user;
                new MainWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().ShowDialog();
        }
    }
}
