using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Institute
{
    internal static class DBConnection
    {
        private static MySqlConnection _mySqlConnection = null;
        private static string _connectionString = "Server=141.8.192.151;Database=f0608526_ppsubdiz;port=3306;User Id=f0608526_ppsubdiz;password=ppsubdiz";

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

        public static void AddData(string query)
        {
            try
            {
                _mySqlConnection.Open();

                MySqlCommand command = new MySqlCommand(query, _mySqlConnection);
                command.ExecuteNonQuery();

                MessageBox.Show("Поздравляем!", "Запись успешно добавлена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось добавить данные в базу.");
            }
            finally
            {
                _mySqlConnection.Close();
            }
        }

        public static DataTable SelectData(string query)
        {
            DataTable table = null;
            try
            {
                _mySqlConnection.Open();
                MySqlCommand command = new MySqlCommand(query, _mySqlConnection);
                command.ExecuteNonQuery();

                table = new DataTable();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка получения данных");
            }
            finally
            {
                _mySqlConnection.Close();
            }
            return table;
        }
    }
}
