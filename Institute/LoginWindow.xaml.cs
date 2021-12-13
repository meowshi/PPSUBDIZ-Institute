using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Institute
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DBConnection.Connect();
        }

        private void butLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gLogin)) return;

            string query = $"select * from user where login = '{tbLogin.Text}' and password = '{pbPassword.Password}'";
            var table = DBConnection.SelectData(query);
            if (table == null || table.Rows.Count == 0)
            {
                MessageBox.Show("Неверно введен логин или пароль", "Внимание");
            }
            else
            {
                User.Login = table.Rows[0]["login"].ToString();
                User.Surname = table.Rows[0]["surname"].ToString();
                User.Name = table.Rows[0]["name"].ToString();
                User.Patronymic = table.Rows[0]["patronymic"].ToString();
                User.PhoneNumber = table.Rows[0]["phone_number"].ToString();
                User.Email = table.Rows[0]["email"].ToString();
                User.AccessLevel = table.Rows[0]["access_level"].ToString();

                string[] dateTime = DateTime.Now.ToString().Split(' ');
                string log_query = $"insert into log (date, time, user_login, action, table_key)  values ('{dateTime[0]}', '{dateTime[1]}', '{User.Login}', 'Вход в систему', 'none')";
                DBConnection.AddData(log_query);

                OpenMainWindow();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            User.AccessLevel = "Zero";

            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private bool IsFieldsFilled(Grid grid, bool showMB = true)
        {
            var childs = LogicalTreeHelper.GetChildren(grid);

            foreach (var child in childs)
            {
                if (child is TextBox textBox && textBox.Text.Equals(String.Empty))
                {
                    if (showMB)
                    {
                        MessageBox.Show("Не все поля заполнены!", "Внимание!");
                    }
                    return false;
                }
                else if (child is DatePicker datePicker && datePicker.Text.Equals(String.Empty))
                {
                    if (showMB)
                    {
                        MessageBox.Show("Не все поля даты заполнены!", "Внимание!");
                    }
                    return false;
                }
                else if (child is PasswordBox passwordBox && passwordBox.Password.ToString().Equals(""))
                {
                    if (showMB)
                    {
                        MessageBox.Show("Не все поля заполнены", "Внимаение");
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
