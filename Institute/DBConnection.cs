using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Institute
{
    internal static class DBConnection
    {
        private static MySqlConnection _mySqlConnection = null;
        private static string _connectionString = "SERVER=localhost;DATABASE=institute;UID=root;PASSWORD=d18594604";

        /// <summary>
        /// Подключение к базе данных. При исключении показывает MessageBox c сообщением из исключения.
        /// </summary>
        public static void Connect()
        {
            try
            {
                _mySqlConnection = new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к базе данных.", ex.Message);
            }
        }
    }
}
