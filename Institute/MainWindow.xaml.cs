using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        const string PassportColumnsStr = "series,number,issue_date,expiry_date,issuing_authority";

        private readonly string[] FacultyFields = { "department_name" };
        private readonly string[] ChairFields = { "faculty_name" };
        private readonly string[] SpecialityFields = { "faculty_name" };
        private readonly string[] DisciplineFields = { "chair_name" };
        private readonly string[] EmployeeFields = { "surname", "name", "patronymic", "inn", "phone_number", "email", "post", "salary", "department_name", "passport_data_id" };
        private readonly string[] TeacherFields = { "chair_name", "academic_rank" };
        private readonly string[] GroupFields = { "faculty_name" };
        private readonly string[] StudentFields = { "surname", "name", "patronymic", "inn", "phone_number", "email", "speciality_name", "chair_name", "group_name", "start_date", "end_date", "education_cost", "passport_data_id" };
        private readonly string[] EnrolleeFields = { "surname", "name", "patronymic", "document_type", "total_score", "passport_data_id" };
        private readonly string[] PassportColumns = PassportColumnsStr.Split(',');
        private readonly string[] UserFields = { "surname", "name", "patronymic", "phone_number", "email", "access_level", "password" };

        private readonly UIElement[] ElementsToDisableZero;
        private readonly UIElement[] ElementsToDisableMinimal;
        private readonly UIElement[] ElementsToDisableTeacher;
        private readonly UIElement[] ElementsToDisableChairHead;
        private readonly UIElement[] ElementsToDisableFacultyHead;
        private readonly UIElement[] ElementsToDisableRector;
        private readonly UIElement[] ElementsToDisableHR;
        private readonly UIElement[] ElementsToDisableAdmissionCommittee;

        public MainWindow()
        {
            InitializeComponent();

            // Подключаемся к базе данных.
            DBConnection.Connect();

            DisableTabs();

            ElementsToDisableZero = new UIElement[] { mainSubsystemButton, reportSubsystemButton, controlSubsystemButton };
            ElementsToDisableMinimal = new UIElement[] { mainSubsystemButton, controlSubsystemButton, tabSearchEmployee, tabSearchTeacher, tabSearchStudent, tabSearchEnrollee };
            ElementsToDisableTeacher = new UIElement[] { mainSubsystemButton, controlSubsystemButton, tabSearchEmployee, tabSearchEnrollee };
            
            ElementsToDisableChairHead = new UIElement[] { controlSubsystemButton, tabSearchEmployee, tabSearchEnrollee, tabAddDepartment, tabAddFaculty, tabAddChair, tabAddSpeciality,
            tabAddEmployee, tabAddTeacher, tabAddGroup, tabAddStudent, tabAddEnrollee, tabAddEnrolleeSpec, tabChangeFaculty, tabChangeChair, tabChangeSpeciality,
            tabChangeEmployee, tabChangeTeacher, tabChangeGroup,tabChangeStudent,tabChangeEnrollee, tabChangeEnrolleeSpec };
            
            ElementsToDisableFacultyHead = new UIElement[] { controlSubsystemButton, tabSearchEmployee, tabSearchEnrollee, tabAddDepartment, tabAddFaculty,
            tabAddDiscipline, tabAddEmployee, tabAddTeacher, tabAddGroup, tabAddEnrollee, tabAddDiscTeacher, tabAddEnrolleeSpec, tabChangeFaculty,
            tabChangeDiscipline, tabChangeEmployee, tabChangeTeacher, tabChangeGroup, tabChangeEnrollee, tabChangeDiscTeacher, tabChangeEnrolleeSpec };

            ElementsToDisableRector = new UIElement[] { tabAddChair, tabAddSpeciality, tabAddDiscipline, tabAddEmployee, tabAddTeacher, tabAddGroup, tabAddStudent, tabAddEnrollee, 
            tabAddDiscTeacher, tabAddEnrolleeSpec, tabChangeChair, tabChangeSpeciality, tabChangeChair, tabChangeDiscipline, tabChangeEmployee, tabChangeTeacher, tabChangeStudent, tabChangeGroup, 
            tabChangeEnrollee, tabChangeDiscTeacher, tabChangeEnrolleeSpec };

            ElementsToDisableHR = new UIElement[] { controlSubsystemButton, tabSearchTeacher, tabSearchEnrollee, tabAddDepartment, tabAddFaculty,
            tabAddChair, tabAddSpeciality, tabAddDiscipline, tabAddGroup, tabAddEnrollee, tabAddDiscTeacher, tabAddEnrolleeSpec, tabChangeFaculty,
            tabChangeChair, tabChangeSpeciality, tabChangeDiscipline, tabChangeGroup, tabChangeEnrollee, tabChangeDiscTeacher, tabChangeEnrolleeSpec };

            ElementsToDisableAdmissionCommittee = new UIElement[] { controlSubsystemButton, tabSearchEmployee, tabSearchTeacher,  tabAddDepartment, tabAddFaculty,
            tabAddChair, tabAddSpeciality, tabAddEmployee, tabAddEnrollee, tabAddDiscipline, tabAddTeacher, tabAddDiscTeacher, tabChangeFaculty,
            tabChangeChair, tabChangeSpeciality, tabChangeDiscipline, tabChangeEmployee, tabChangeTeacher, tabChangeDiscTeacher };

            switch (User.AccessLevel)
            {
                case "Zero":
                    DisableElements(ElementsToDisableZero);
                    break;
                case "Minimal":
                    DisableElements(ElementsToDisableMinimal);
                    break;
                case "Teacher":
                    DisableElements(ElementsToDisableTeacher);
                    break;
                case "ChairHead":
                    DisableElements(ElementsToDisableChairHead);
                    tcAdd.SelectedItem = tabAddDiscipline;
                    tcChange.SelectedItem = tabChangeDiscipline;
                    break;
                case "FacultyHead":
                    DisableElements(ElementsToDisableFacultyHead);
                    tcAdd.SelectedItem = tabAddChair;
                    tcChange.SelectedItem = tabChangeChair;
                    break;
                case "Rector":
                    DisableElements(ElementsToDisableRector);
                    break;
                case "HR":
                    DisableElements(ElementsToDisableHR);
                    tcAdd.SelectedItem = tabAddEmployee;
                    tcChange.SelectedItem = tabChangeEmployee;
                    break;
                case "AdmissionCommittee":
                    DisableElements(ElementsToDisableAdmissionCommittee);
                    tcAdd.SelectedItem = tabAddStudent;
                    tcChange.SelectedItem = tabChangeStudent;
                    break;
            }
        }

        private void DisableElements(UIElement[] elements)
        {
            foreach (UIElement element in elements)
            {
                element.IsEnabled = false;
            }
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
                if (child is TextBox textBox)
                {
                    textBox.Clear();
                }
                else if (child is DatePicker datePicker)
                {
                    datePicker.Text = "";
                }
                else if (child is PasswordBox passwordBox)
                {
                    passwordBox.Clear();
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
                else if (child is PasswordBox passwordBox && !passwordBox.Password.ToString().Equals(String.Empty))
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
                }
                else if (child is DatePicker datePicker)
                {
                    querySB.Append($"'{datePicker.Text}',");
                }
                else if (child is PasswordBox passwordBox)
                {
                    querySB.Append($"'{passwordBox.Password}',");
                }
            }

            querySB.Remove(querySB.Length - 1, 1);
            querySB.Append(')');

            Trace.WriteLine(querySB.ToString());

            return querySB.ToString();
        }

        /// <summary>
        /// Производит добавление записи.
        /// </summary>
        /// <param name="table">Название таблицы для записи.</param>
        /// <param name="grid">Грид, из которого беруться данные.</param>
        /// <param name="columns">Опцинально. Если запись не во все поля таблицы (когда, например, айдишник сам пишется), то нужно указать порядок столбцов.</param>
        private bool Add(string table, Grid grid, string columns = "")
        {
            if (!IsFieldsFilled(grid)) return false;

            string query = MakeAddQuery(table, grid, columns);
            return DBConnection.AddData(query);
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

            LoadLog();
        }

        private void LoadLog()
        {
            tbConrolLog.Text = "";

            string query = "select date, time, action, table_key, surname, name, patronymic from log join user on user_login = login";
            var table = DBConnection.SelectData(query);
            foreach(DataRow row in table.Rows)
            {
                string log = $" [{row["date"]} {row["time"]}] {row["surname"]} {row["name"]} {row["patronymic"]}. {row["action"]} (Ключ: {row["table_key"]}).\n";
                tbConrolLog.Text += log;
            }

            svControlLog.ScrollToBottom();
        }

        private void feedbackSubsystemButton_Click(object sender, RoutedEventArgs e)
        {
            feedbackSubsystemGrid.Visibility = Visibility.Visible;

            mainSubsystemTab.Visibility = Visibility.Collapsed;
            repotSubsystemTab.Visibility = Visibility.Collapsed;
            controlSubsystemTab.Visibility = Visibility.Collapsed;
        }

        private void AddLog(string action, string table_key)
        {
            string[] dateTime = DateTime.Now.ToString().Split(' ');

            string query = $"insert into log (date, time, user_login, action, table_key) values ('{dateTime[0]}','{dateTime[1]}','{User.Login}','{action}','{table_key}')";
            DBConnection.AddData(query);
        }

        private void AddErrorMessage()
        {
            MessageBox.Show("Ошибка добавления записи!", "Внимание!");
        }

        private void AddSuccessMessage(string key)
        {
            MessageBox.Show($"Запись успешно добавлена!\n(Ключ: {key})");
        }

        private void butAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("department", gAddDepartment))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление отдела", tbAddDepartmentName.Text);
            AddSuccessMessage(tbAddDepartmentName.Text);
        }

        private void butClearAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddDepartment);
        }

        private void butAddFaculty_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("faculty", gAddFaculty))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление факультета", tbAddFacultyName.Text);
            AddSuccessMessage(tbAddFacultyName.Text);
        }

        private void butClearAddFaculty_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddFaculty);
        }

        private void butAddChair_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("chair", gAddChair))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление кафедры", tbAddChairName.Text);
            AddSuccessMessage(tbAddChairName.Text);
        }

        private void butClearAddChair_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddChair);
        }

        private void butAddSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("speciality", gAddSpeciality))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление направления подготовки", tbAddSpecialityName.Text);
            AddSuccessMessage(tbAddSpecialityName.Text);
        }

        private void butClearAddSpeciality_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddSpeciality);
        }

        private void butAddDiscipline_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("discipline", gAddDiscipline))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление дисциплины", tbAddDisciplineName.Text);
            AddSuccessMessage(tbAddDisciplineName.Text);
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
            if (!DBConnection.AddData(passportQuery))
            {
                AddErrorMessage();
                return;
            }
            
            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddEmployeePassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string employeeColumns = "surname,name,patronymic,inn, phone_number,email,post,salary,department_name, passport_data_id";
            string employeeValues = $"'{tbAddEmployeeSurname.Text}', '{tbAddEmployeeName.Text}', '{tbAddEmployeePatronymic.Text}' , '{tbAddEmployeeINN.Text}', '{tbAddEmployeePhoneNumber.Text}'," +
                                     $"'{tbAddEmployeeEmail.Text}', '{tbAddEmployeePost.Text}', '{tbAddEmployeeSalary.Text}', '{tbAddEmployeeDepartmentName.Text}', '{passportId}'";
            string employeeQuery = $"insert into employee ({employeeColumns}) values ({employeeValues})";
            if (!DBConnection.AddData(employeeQuery))
            {
                DBConnection.ChangeData($"delete from passport_data where id = '{passportId}'");
                AddErrorMessage();
                return;
            }
            string query = $"select id from employee where passport_data_id = '{passportId}'";
            DataTable employeeIdTable = DBConnection.SelectData(query);
            string employeeId = employeeIdTable.Rows[0][0].ToString();

            AddLog("Добавление паспортных данных", passportId);
            AddLog("Добавление сотрудника", employeeId);

            AddSuccessMessage(employeeId);
        }

        private void butClearAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEmployee);
        }

        private void butAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("teacher", gAddTeacher))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление преподавателя", tbAddTeacherEmployeeId.Text);
            AddSuccessMessage(tbAddTeacherEmployeeId.Text);
        }

        private void butClearAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddTeacher);
        }

        private void butAddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gAddGroup)) return;

            string query = $"insert into `group` values ('{tbAddGroupName.Text}', '{tbAddGroupFacultyName.Text}')";
            if (!DBConnection.AddData(query))
            {
                AddErrorMessage();
                return;
            }

            AddLog("Добавление группы", tbAddGroupName.Text);
            AddSuccessMessage(tbAddGroupName.Text);
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
            if (!DBConnection.AddData(passportQuery))
            {
                AddErrorMessage();
                return;
            }

            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddStudentPassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string studentColumns = "surname,name,patronymic,inn,phone_number,email,speciality_name,chair_name,group_name,start_date,end_date,education_cost,passport_data_id";
            string studentValues = $"'{tbAddStudentSurname.Text}', '{tbAddStudentName.Text}', '{tbAddStudentPatronymic.Text}' , '{tbAddStudentINN.Text}', '{tbAddStudentPhoneNumber.Text}'," +
                $"'{tbAddStudentEmail.Text}', '{tbAddStudentSpecialityName.Text}', '{tbAddStudentChairName.Text}', '{tbAddStudentGroupName.Text}', '{dpAddStudentStartDate.Text}'," +
                $"'{dpAddStudentEndDate.Text}', '{tbAddStudentEduCost.Text}', '{passportId}'";
            string studentQuery = $"insert into student ({studentColumns}) values ({studentValues})";
            if (!DBConnection.AddData(studentQuery))
            {
                DBConnection.ChangeData($"delete from passport_data where id = '{passportId}'");
                AddErrorMessage();
                return;
            }

            string query = $"select id from student where passport_data_id = '{passportId}'";
            DataTable studentIdTable = DBConnection.SelectData(query);
            string studentId = studentIdTable.Rows[0][0].ToString();

            AddLog("Добавление паспортных данных", passportId);
            AddLog("Добавление студента", studentId);

            AddSuccessMessage(studentId);
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
            if (!DBConnection.AddData(passportQuery))
            {
                AddErrorMessage();
                return;
            }

            var table = DBConnection.SelectData($"select id from passport_data where number = '{tbAddEnrolleePassportNumber.Text}'");
            string passportId = table.Rows[0][0].ToString();

            string enrolleeColumns = "surname,name,patronymic,document_type,total_score,passport_data_id";
            string enrolleeValues = $"'{tbAddEnrolleeSurname.Text}','{tbAddEnrolleeName.Text}','{tbAddEnrolleePatronymic.Text}'," +
                $"'{tbAddEnrolleeDocumentType.Text}','{tbAddEnrolleeTotalScore.Text}', '{passportId}'";
            string enrolleeQuery = $"insert into enrollee ({enrolleeColumns}) values ({enrolleeValues})";
            if (!DBConnection.AddData(enrolleeQuery))
            {
                DBConnection.ChangeData($"delete from passport_data where id = '{passportId}'");
                AddErrorMessage();
                return;
            }

            string query = $"select id from enrollee where passport_data_id = '{passportId}'";
            DBConnection.SelectData(query);
            var enrolleeIdTable = DBConnection.SelectData(query);
            string enrolleeId = enrolleeIdTable.Rows[0][0].ToString();

            AddLog("Добавление паспортных данных", passportId);
            AddLog("Добавление студента", enrolleeId);

            AddSuccessMessage(enrolleeId);
        }

        private void butClearAddEnrollee_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEnrollee);
        }

        private void butAddTeacherDisc_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("discipline_teacher", gAddTeacherDisc))
            {
                AddErrorMessage();
                return;
            }
            string key = $"{tbAddTeacherDiscTeacherId.Text}, {tbAddTeacherDiscDiscName.Text}";
            
            AddLog("Добавление преподавателя и дисциплины", key);
            AddSuccessMessage(key);
        }

        private void butClearAddTeacherDisc_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddTeacherDisc);
        }

        private void butAddEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("enrollee_speciality", gAddEnrolleeSpec))
            {
                AddErrorMessage();
                return;
            }
            string key = $"{tbAddEnrolleeSpecEnrolleeId.Text}, {tbAddEnrolleeSpecSpecName.Text}";

            AddLog("Добавление абитуриента и преподавателя", key);
            AddSuccessMessage(key);
        }

        private void butClearAddEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEnrolleeSpec);
        }

        /// <summary>
        /// Меняет или удаляет данные.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="keyColumnName"></param>
        /// <param name="key"></param>
        /// <param name="canDelete"></param>
        /// <returns>0 - ничего не произошло, 1 - удаление, 2 - изменение</returns>
        private int Change(Grid grid, string table, string[] columns, string keyColumnName, string key, bool canDelete = true)
        {
            bool isOtherFieldsNotEmpty = IsFieldsNotEmpty(grid);
            if (!isOtherFieldsNotEmpty)
            {
                if (canDelete)
                {
                    string query = $"delete from {table} where {keyColumnName} = '{key}'";
                    if (DBConnection.ChangeData(query))
                    {
                        return 1;
                    }
                }

                return 0;
            }
            else
            {
                StringBuilder querySB = new StringBuilder($"update {table} set ");

                var childs = LogicalTreeHelper.GetChildren(grid);

                int i = 0;
                foreach (var child in childs)
                {
                    if (child is TextBox textBox)
                    {
                        if (!textBox.Text.Equals(""))
                        {
                            querySB.Append(columns[i] + $" = '{textBox.Text}',");
                        }
                        i++;
                    }
                    else if (child is DatePicker datePicker)
                    {
                        if (!datePicker.Text.Equals(""))
                        {
                            querySB.Append(columns[i] + $" = '{datePicker.Text}',");
                        }
                        i++;
                    }
                    else if (child is PasswordBox passwordBox)
                    {
                        if (!passwordBox.Password.ToString().Equals(""))
                        {
                            querySB.Append(columns[i] + $" = '{passwordBox.Password}',");
                        }
                        i++;
                    }
                }
                querySB.Remove(querySB.Length - 1, 1);
                querySB.Append($" where {keyColumnName} = '{key}'");
                Trace.WriteLine(querySB.ToString());
                if (DBConnection.ChangeData(querySB.ToString()))
                {
                    return 2;
                }
                return 0;
            }
        }

        private void ChangeSuccessMessage()
        {
            MessageBox.Show("Изменение прошло успешно!", "Ура!");
        }

        private void DeleteSuccessMessage()
        {
            MessageBox.Show("Удаление прошло успешно!", "Ура!");
        }

        private void ChangeErrorMessage()
        {
            MessageBox.Show("При изменении произошла ошибка!", "Внимание!");
        }

        private void ChangeNotify(int result, string target, string key)
        {
            switch (result)
            {
                case 0:
                    ChangeErrorMessage();
                    break;
                case 1:
                    DeleteSuccessMessage();
                    AddLog($"Удаление {target}", key);
                    break;
                case 2:
                    ChangeSuccessMessage();
                    AddLog($"Изменение {target}", key);
                    break;
            }
        }

        private void butChangeFaculty_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeFacultyName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeFacultyOther, "faculty", FacultyFields, "name", tbChangeFacultyName.Text);
            ChangeNotify(result, "факультета", tbChangeFacultyName.Text);
        }

        private void butClearChangeFaculty_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeFaculty);
            ClearGrid(gChangeFacultyOther);
        }

        private void butChangeChair_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeChairName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeChairOther, "chair", ChairFields, "name", tbChangeChairName.Text);
            ChangeNotify(result, "кафедры", tbChangeChairName.Text);
        }

        private void butClearChangeChair_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeChair);
            ClearGrid(gChangeChairOther);
        }

        private void butChangeSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeSpecialityName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeSpecialityOther, "speciality", SpecialityFields, "name", tbChangeSpecialityName.Text);
            ChangeNotify(result, "направления подготовки", tbChangeSpecialityName.Text);
        }

        private void butClearChangeSpeciality_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeSpeciality);
            ClearGrid(gChangeSpecialityOther);
        }

        private void butChangeDiscipline_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeDisciplineName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeDisciplineOther, "discipline", DisciplineFields, "name", tbChangeDisciplineName.Text);
            ChangeNotify(result, "дисциплины", tbChangeDisciplineName.Text);
        }

        private void butClearChangeDiscipline_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeDiscipline);
            ClearGrid(gChangeDisciplineOther);
        }

        private void butChangeEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeEmployeeId.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            string query = $"select passport_data_id from employee where id = '{tbChangeEmployeeId.Text}'";
            var employeeTable = DBConnection.SelectData(query);
            if (employeeTable.Rows.Count == 0)
            {
                MessageBox.Show("Неверный код!", "Внимание!");
                return;
            }

            string passportDataId = employeeTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeEmployeePassport) || IsFieldsNotEmpty(gChangeEmployeeOther));

            int result = Change(gChangeEmployeeOther, "employee", EmployeeFields, "id", tbChangeEmployeeId.Text, canDelete);
            ChangeNotify(result, "сотрудника", tbChangeEmployeeId.Text);
            
            result = Change(gChangeEmployeePassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
            ChangeNotify(result, "паспортных данных", passportDataId);
        }

        private void butClearChangeEmployee1_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeEmployee);
            ClearGrid(gChangeEmployeeOther);
            ClearGrid(gChangeEmployeePassport);
        }

        private void butChangeTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeTeacherEmployeeId.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeTeacherOther, "teacher", TeacherFields, "employee_id", tbChangeTeacherEmployeeId.Text);
            ChangeNotify(result, "преподавателя", tbChangeTeacherEmployeeId.Text);
        }

        private void butClearChangeTeacher_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeTeacher);
            ClearGrid(gChangeTeacherOther);
        }

        private void butChangeGroup_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeGroupName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            int result = Change(gChangeGroupOther, "`group`", GroupFields, "name", tbChangeGroupName.Text);
            ChangeNotify(result, "группы", tbChangeGroupName.Text);
        }

        private void butClearChangeGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeGroup);
            ClearGrid(gChangeGroupOther);
        }

        private void butChangeStudent_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeStudentId.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            string query = $"select passport_data_id from student where id = '{tbChangeStudentId.Text}'";
            var studentTable = DBConnection.SelectData(query);
            if (studentTable.Rows.Count == 0)
            {
                MessageBox.Show("Неверный код!", "Внимание!");
                return;
            }

            string passportDataId = studentTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeStudentPassport) || IsFieldsNotEmpty(gChangeStudentOther));

            int result = Change(gChangeStudentOther, "student", StudentFields, "id", tbChangeStudentId.Text, canDelete);
            ChangeNotify(result, "студента", tbChangeStudentId.Text);

            result = Change(gChangeStudentPassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
            ChangeNotify(result, "паспортных данных", passportDataId);
        }

        private void butClearChangeStudent_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeStudent);
            ClearGrid(gChangeStudentOther);
            ClearGrid(gChangeStudentPassport);
        }

        private void butChangeEnrollee_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeEnrolleeId.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            string query = $"select passport_data_id from enrollee where id = '{tbChangeEnrolleeId.Text}'";
            var enrolleeTable = DBConnection.SelectData(query);
            if (enrolleeTable.Rows.Count == 0)
            {
                MessageBox.Show("Неверный код!", "Внимание!");
                return;
            }
            string passportDataId = enrolleeTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeEnrolleePassport) || IsFieldsNotEmpty(gChangeEnrolleeOther));

            int result = Change(gChangeEnrolleeOther, "enrollee", EnrolleeFields, "id", tbChangeEnrolleeId.Text, canDelete);
            ChangeNotify(result, "абитуриента", tbChangeEnrolleeId.Text);

            result = Change(gChangeEnrolleePassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
            ChangeNotify(result, "паспортных данных", passportDataId);
        }

        private void butClearChangeEnrollee_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeEnrollee);
            ClearGrid(gChangeEnrolleeOther);
            ClearGrid(gChangeEnrolleePassport);
        }

        private void butFeedbackSend_Click(object sender, RoutedEventArgs e)
        {
            string message = tbFeedbackMessage.Text;
            if (message == "")
            {
                MessageBox.Show("Вы не ввели сообщение!", "Внимание!");
                return;
            }

            var from = new MailAddress("topwowerintheworld@gmail.com", "INSTITUTE");
            var to = new MailAddress("denlas31@gmail.com", "Glava");
            const string fromPassword = "qvbqbebiycujtedc";
            const string subject = "Обратная связь";
            string body = message;

            try
            {
                var smtpClient = new SmtpClient
                {
                    Host = "smtp.google.com",
                    Port = 25,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from.Address, fromPassword)
                };

                using (var mailMessage = new MailMessage(from, to) { Subject = subject, Body = body })
                {
                    smtpClient.Send(mailMessage);
                }

                MessageBox.Show("Сообщение успешно отправлено!", "Ура!");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Что-то пошло не так...");
            }
        }

        private void butChangeDiscTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsNotEmpty(gChangeDiscTeacher))
            {
                MessageBox.Show("Вы не заполнили поля!", "Внимание!");
                return;
            }

            string disciplineName = tbCangeTeacherDiscDiscName.Text;
            string teacherId = tbChangeTeacherDiscTeacherId.Text;

            if (disciplineName != "" && teacherId != "")
            {
                string query = $"delete from discipline_teacher where discipline_name = '{disciplineName}' and employee_id = '{teacherId}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление дисциплины и преподавателя", $"{disciplineName}, {teacherId}");
                    return;
                }
            }

            else if (disciplineName != "")
            {
                string query = $"delete from discipline_teacher where discipline_name = '{disciplineName}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление преподавателей дисциплины", disciplineName);
                    return;
                }
            }

            else
            {
                string query = $"delete from discipline_teacher where employee_id = '{teacherId}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление дисциплин преподавателя", teacherId);
                    return;
                }
            } 
            

            MessageBox.Show("При удалении данных произошла ошбика!", "Внимание!");
        }

        private void butClearChangeDiscTeacher_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeDiscTeacher);
        }

        private void butChangeEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsNotEmpty(gChangeEnrolleeSpec))
            {
                MessageBox.Show("Вы не заполнили поля!", "Внимание!");
                return;
            }

            string enrolleeId = tbChangeEnrolleeSpecEnrolleeId.Text;
            string specName = tbChangeEnrolleeSpecSpecName.Text;

            if (enrolleeId != "" && specName != "")
            {
                string query = $"delete from enrollee_speciality where enrollee_id = '{enrolleeId}' and speciality_name = '{specName}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление абитуриента и направление подготовки", $"{enrolleeId}, {specName}");
                    return;
                }
            }

            else if (enrolleeId != "")
            {
                string query = $"delete from enrollee_speciality where enrollee_id = '{enrolleeId}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление направлений подготовки абитуриента!", enrolleeId);
                    return;
                }
            }

            else
            {
                string query = $"delete from enrollee_speciality where speciality_name = '{specName}'";
                if (DBConnection.ChangeData(query))
                {
                    MessageBox.Show("Данные успешно удалены!", "Ура!");
                    AddLog("Удаление абитуриентов направления", specName);
                    return;
                }
            }

            MessageBox.Show("При удалении данных произошла ошбика!", "Внимание!");
        }

        private void butClearChangeEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeEnrolleeSpec);
        }

        private void butControlAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!Add("user", gControlAdd))
            {
                AddErrorMessage();
                return;
            }
            AddLog("Добавление пользователя", tbControlAddLogin.Text);
        }

        private void butClearControlAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gControlAdd);
        }

        private void butControlChange_Click(object sender, RoutedEventArgs e)
        {
            if (tbControlChangeLogin.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            string login = tbControlChangeLogin.Text;

            int result = Change(gControlChangeOther, "user", UserFields, "login", login);
            ChangeNotify(result, "пользователя", login);
        }

        private void butClearControlChange_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gControlChange);
            ClearGrid(gControlChangeOther);
        }

        private void butControlLog_Click(object sender, RoutedEventArgs e)
        {
            if (!DBConnection.ChangeData("delete from log"))
            {
                MessageBox.Show("При удалении произошла ошибка!", "Внимание!");
                return;
            }

            AddLog("Очистка журнала", "none");
            LoadLog();
        }
    }
}
