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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Institute
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        const string PassportColumns = "series,number,issue_date,expiry_date,issuing_authority";

        private readonly string[] FacultyFields = new string[] { "department_name" };
        


        public MainWindow()
        {
            InitializeComponent();

            // Подключаемся к базе данных.
            DBConnection.Connect();

            DisableTabs();
        }

        public void NotImplemented()
        {
            MessageBox.Show("НЕ реализовал!");
        }

        /// <summary>
        /// Очищает все поля ввода в гриде.
        /// </summary>
        /// <param name="grid">Грид, в которм надо очистить поля.</param>
        private void ClearGrid(Grid grid)
        {
            var childs = LogicalTreeHelper.GetChildren(grid);
            
            foreach (var child in childs)
            {
                var textBox = child as TextBox;
                if (textBox != null)
                {
                    textBox.Clear();
                }
                else
                {
                    var datePicker = child as DatePicker;
                    if (datePicker != null)
                    {
                        datePicker.Text = String.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет все поля ввода в гриде на пустоту.
        /// </summary>
        /// <param name="grid">Грид, в которм проверяются поля.</param>
        /// <returns></returns>
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
            }

            return true;
        }

        private bool IsFieldsNotEmpty(Grid grid)
        {
            var childs = LogicalTreeHelper.GetChildren(grid);

            foreach (var child in childs)
            {
                if (child is TextBox textBox && !textBox.Text.Equals(String.Empty))
                {
                    return true;
                }
                else if (child is DatePicker datePicker && !datePicker.Text.Equals(String.Empty))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Удобная штука но только если нужно делать запрос для всех полей на форме.
        /// То есть для студента, например, не подходит так как там нужно и в паспорт и в студента.
        /// Но сейчас подумал, что можно еще два разных грида запихнуть и из них собирать текст боксы.
        /// Может так и сделаю если будет время.
        /// </summary>
        /// <param name="table">Название таблицы.</param>
        /// <param name="grid">Грид где находятся боксы с данными.</param>
        /// <param name="columns">Опцинально. Если запись не во все поля таблицы (когда, например, айдишник сам пишется), то нужно указать порядок столбцов.</param>
        /// <returns></returns>
        private string MakeAddQuery(string table, Grid grid, string columns)
        {
            var querySB = new StringBuilder($"insert into {table} ({columns}) values (");
            
            var childs = LogicalTreeHelper.GetChildren(grid);
            foreach (var child in childs) {
                if (child is TextBox textBox)
                {
                    querySB.Append($"'{textBox.Text}',");
                    Trace.WriteLine(textBox.Text);
                }
                else if (child is DatePicker datePicker)
                {
                    querySB.Append($"'{datePicker.Text}',");
                    Trace.WriteLine(datePicker.Text);
                }
            }

            querySB.Remove(querySB.Length - 1, 1);
            querySB.Append(')');

            return querySB.ToString();
        }

        /// <summary>
        /// Производит добавление записи.
        /// </summary>
        /// <param name="table">Название таблицы для записи.</param>
        /// <param name="grid">Грид, из которого беруться данные.</param>
        /// <param name="columns">Опцинально. Если запись не во все поля таблицы (когда, например, айдишник сам пишется), то нужно указать порядок столбцов.</param>
        private void Add(string table, Grid grid, string columns = "")
        {
            if (!IsFieldsFilled(grid)) return;

            string query = MakeAddQuery(table, grid, columns);
            DBConnection.AddData(query);
        }

        /// <summary>
        /// Прячет все вкладки. Далее они будут включаться/отключаться нажатием вкладок меню.
        /// </summary>
        private void DisableTabs()
        {
            mainSubsystemTab.Visibility = Visibility.Collapsed;
            repotSubsystemTab.Visibility = Visibility.Collapsed;
            controlSubsystemTab.Visibility = Visibility.Collapsed;
            feedbackSubsystemGrid.Visibility = Visibility.Collapsed;
        }

        private void mainSubsystemButton_Click(object sender, RoutedEventArgs e)
        {
            mainSubsystemTab.Visibility = Visibility.Visible;

            repotSubsystemTab.Visibility = Visibility.Collapsed;
            controlSubsystemTab.Visibility = Visibility.Collapsed;
            feedbackSubsystemGrid.Visibility = Visibility.Collapsed;
        }

        private void reportSubsystemButton_Click(object sender, RoutedEventArgs e)
        {
            repotSubsystemTab.Visibility = Visibility.Visible;

            mainSubsystemTab.Visibility = Visibility.Collapsed;
            controlSubsystemTab.Visibility = Visibility.Collapsed;
            feedbackSubsystemGrid.Visibility = Visibility.Collapsed;
        }

        private void controlSubsystemButton_Click(object sender, RoutedEventArgs e)
        {
            controlSubsystemTab.Visibility = Visibility.Visible;

            mainSubsystemTab.Visibility = Visibility.Collapsed;
            repotSubsystemTab.Visibility = Visibility.Collapsed;
            feedbackSubsystemGrid.Visibility = Visibility.Collapsed;
        }

        private void feedbackSubsystemButton_Click(object sender, RoutedEventArgs e)
        {
            feedbackSubsystemGrid.Visibility = Visibility.Visible;

            mainSubsystemTab.Visibility = Visibility.Collapsed;
            repotSubsystemTab.Visibility = Visibility.Collapsed;
            controlSubsystemTab.Visibility = Visibility.Collapsed;
        }

        private void butAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            Add("department", gAddDepartment);
        }

        private void butClearAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddDepartment);
        }

        private void butAddFaculty_Click(object sender, RoutedEventArgs e)
        {
            Add("faculty", gAddFaculty);
        }

        private void butClearAddFaculty_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddFaculty);
        }

        private void butAddChair_Click(object sender, RoutedEventArgs e)
        {
            Add("chair", gAddChair);
        }

        private void butClearAddChair_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddChair);
        }

        private void butAddSpeciality_Click(object sender, RoutedEventArgs e)
        {
            Add("speciality", gAddSpeciality);
        }

        private void butClearAddSpeciality_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddSpeciality);
        }

        private void butAddDiscipline_Click(object sender, RoutedEventArgs e)
        {
            Add("discipline", gAddDiscipline);
        }

        private void butClearAddDiscipline_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddDiscipline);
        }

        private void butAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gAddEmployee)) return;

            // Сначала добавляем паспорт так как надо получить код паспортных данных.
            string passportValues = $"'{tbAddEmployeePassportSeries.Text}','{tbAddEmployeePassportNumber.Text}','{dpAddEmployeePassportIssueDate.Text}','{dpAddEmployeePassportExpiryDate.Text}','{tbAddEmployeePassportIssuingAuthority.Text}'";
            string passportQuery = $"insert into passport_data ({PassportColumns}) values ({passportValues})";
            DBConnection.AddData(passportQuery);
            
            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddEmployeePassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string employeeColumns = "surname,name,patronymic,inn, phone_number,email,post,salary,department_name, passport_data_id";
            string employeeValues = $"'{tbAddEmployeeSurname.Text}', '{tbAddEmployeeName.Text}', '{tbAddEmployeePatronymic.Text}' , '{tbAddEmployeeINN.Text}', '{tbAddEmployeePhoneNumber.Text}'," +
                                     $"'{tbAddEmployeeEmail.Text}', '{tbAddEmployeePost.Text}', '{tbAddEmployeeSalary.Text}', '{tbAddEmployeeDepartmentName.Text}', '{passportId}'";
            string employeeQuery = $"insert into employee ({employeeColumns}) values ({employeeValues})";
            DBConnection.AddData(employeeQuery);
        }

        private void butClearAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEmployee);
        }

        private void butAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            Add("teacher", gAddTeacher);
        }

        private void butClearAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddTeacher);
        }

        private void butAddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gAddGroup)) return;

            string query = $"insert into `group` values ('{tbAddGroupName.Text}', '{tbAddGroupFacultyName.Text}')";
            DBConnection.AddData(query);
        }

        private void butClearAddGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddGroup);
        }

        private void butAddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gAddStudent)) return;

            // Сначала добавляем паспорт так как надо получить код паспортных данных.
            string passportValues = $"'{tbAddStudentPassportSeries.Text}','{tbAddStudentPassportNumber.Text}','{dpAddStudentPassportIssueDate.Text}','{dpAddStudentPassportExpiryDate.Text}','{tbAddStudentPassportIssuingAuthority.Text}'";
            string passportQuery = $"insert into passport_data ({PassportColumns}) values ({passportValues})";
            DBConnection.AddData(passportQuery);

            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddStudentPassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string studentColumns = "surname,name,patronymic,inn,phone_number,email,speciality_name,chair_name,group_name,start_date,end_date,education_cost,passport_data_id";
            string studentValues = $"'{tbAddStudentSurname.Text}', '{tbAddStudentName.Text}', '{tbAddStudentPatronymic.Text}' , '{tbAddStudentINN.Text}', '{tbAddStudentPhoneNumber.Text}'," +
                $"'{tbAddStudentEmail.Text}', '{tbAddStudentSpecialityName.Text}', '{tbAddStudentChairName.Text}', '{tbAddStudentGroupName.Text}', '{dpAddStudentStartDate.Text}'," +
                $"'{dpAddStudentEndDate.Text}', '{tbAddStudentEduCost.Text}', '{passportId}'";
            string studentQuery = $"insert into student ({studentColumns}) values ({studentValues})";
            DBConnection.AddData(studentQuery);
        }

        private void butClearAddStudent_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddStudent);
        }

        private void butAddEnrollee_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gAddEnrollee)) return;

            // Сначала добавляем паспорт так как надо получить код паспортных данных.
            string passportValues = $"'{tbAddEnrolleePassportSeries.Text}','{tbAddEnrolleePassportNumber.Text}','{dpAddEnrolleePassportIssueDate.Text}','{dpAddEnrolleePassportExpiryDate.Text}','{tbAddEnrolleePassportIssuingAuthority.Text}'";
            string passportQuery = $"insert into passport_data ({PassportColumns}) values ({passportValues})";
            DBConnection.AddData(passportQuery);

            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddEnrolleePassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string enrolleeColumns = "surname,name,patronymic,document_type,total_score,passport_data_id";
            string enrolleeValues = $"'{tbAddEnrolleeSurname.Text}','{tbAddEnrolleeName.Text}','{tbAddEnrolleePatronymic.Text}'," +
                $"'{tbAddEnrolleeDocumentType.Text}','{tbAddEnrolleeTotalScore.Text}', '{passportId}'";
            string enrolleeQuery = $"insert into enrollee ({enrolleeColumns}) values ({enrolleeValues})";
            DBConnection.AddData(enrolleeQuery);
        }

        private void butClearAddEnrollee_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEnrollee);
        }

        private void Change(Grid grid, string table, string[] columns, string keyColumnName, string key)
        {
            bool isOtherFieldsNotEmpty = IsFieldsNotEmpty(grid);
            if (!isOtherFieldsNotEmpty)
            {
                string query = $"delete from {table} where {keyColumnName} = '{key}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Удаление прошло успешно", "Ура!");
                }

                return;
            }
            else
            {
                StringBuilder querySB = new StringBuilder($"update {table} set ");

                var childs = LogicalTreeHelper.GetChildren(grid);

                int i = 0;
                foreach (var child in childs)
                {
                    if (child is TextBox textBox && !textBox.Text.Equals(""))
                    {
                        querySB.Append(columns[i] + $" = '{textBox.Text}',");
                    }
                    i++;
                }
                querySB.Remove(querySB.Length - 1, 1);
                querySB.Append($" where {keyColumnName} = '{key}'");
                Trace.WriteLine(querySB.ToString());
                if (DBConnection.ChangeData(querySB.ToString()))
                {
                    MessageBox.Show("Изменение прошло успешно!", "Ура!");
                }
            }
        }
        private void butChangeFaculty_Click(object sender, RoutedEventArgs e)
        {
            bool isKeyEmpty = tbChangeFacultyName.Text == "";

            if (isKeyEmpty)
            {
                MessageBox.Show("Вы не заполнили поля!", "Внимание!");
                return;
            }

            Change(gChangeFacultyOther, "faculty", FacultyFields, "name", tbChangeFacultyName.Text);
        }

        private void butClearChangeFaculty_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
