using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace ComputerShop
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fio = tbFio.Text.Trim();
                string login = tbLogin.Text.Trim();
                string email = tbEmail.Text.Trim();
                string phone = tbPhone.Text.Trim();
                string pass = pbPass.Password;
                string pass2 = pbPass2.Password;

                if (fio.Length < 3) { MessageBox.Show("Введите ФИО."); return; }
                if (login.Length < 3) { MessageBox.Show("Логин — не меньше 3 символов."); return; }
                if (pass.Length < 6) { MessageBox.Show("Пароль — не меньше 6 символов."); return; }
                if (pass != pass2) { MessageBox.Show("Пароли не совпадают."); return; }
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Некорректный e-mail.");
                    return;
                }

                if (Db.RegisterUser(login, Db.Sha256(pass), fio, email, phone))
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите.");
                    Close();
                }
                else
                {
                    MessageBox.Show("Такой логин уже занят.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
