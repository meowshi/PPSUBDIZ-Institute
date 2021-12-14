using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Institute.tables;

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
                else if (child is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = 0;
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
                        MessageBox.Show("Не все поля заполнены", "Внимание!");
                    }
                    return false;
                }
                else if (child is ComboBox comboBox && comboBox.Text.Equals("---"))
                {
                    if (showMB)
                    {
                        MessageBox.Show("Не все поля заполнены", "Внимание!");
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
                else if (child is ComboBox comboBox && !comboBox.Text.Equals("---"))
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
                else if (child is ComboBox comboBox)
                {
                    querySB.Append($"'{comboBox.Text}',");
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
                    else if (child is ComboBox comboBox)
                    {
                        if (!comboBox.Text.Equals("---"))
                        {
                            Trace.WriteLine(i);
                            querySB.Append(columns[i] + $" = '{comboBox.Text}',");
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
            MessageBox.Show("Данные не изменены!", "Внимание!");
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

            MailAddress from = new MailAddress("topwowerintheworld@gmail.com", "INSTITUTE");
            MailAddress to = new MailAddress("denlas31@gmail.com", "Glava");
            const string fromPassword = "qvbqbebiycujtedc";
            const string subject = "Обратная связь";
            string body = message;

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential(from.Address, fromPassword);
                smtpClient.EnableSsl = true;
                //smtpClient.Send(m);
                /*var smtpClient = new SmtpClient
                {
                    Host = "smtp.google.com",
                    Port = 25,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from.Address, fromPassword)
                };*/

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

        private void mainSubsystemTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void butSearchDepartmentClick(object sender, RoutedEventArgs e)
        {
            List<Department> department = new List<Department>();

            List<string> where = new List<string>();
            if (tbSearchDepartmentName.Text != "")
                where.Add($"department.name = '{tbSearchDepartmentName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT department.name, count(employee.department_name) FROM department LEFT JOIN employee ON employee.department_name = department.name {String.Join(" AND ", where.ToArray())} GROUP BY department.name; ");

            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    department.Add(new Department() { Name = table.Rows[i][0].ToString(), Staff = table.Rows[i][1].ToString() });
                }

                dgSearchDepartment.ItemsSource = department;
                dgSearchDepartment.Columns[0].Header = "Название";
                dgSearchDepartment.Columns[1].Header = "Штат";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchDepartmentClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(departmentGrid);
        }
        private void butClearSearchFacultyClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(facultyGrid);
        }

        private void butSearchFacultyClick(object sender, RoutedEventArgs e)
        {
            List<Faculty> facultys = new List<Faculty>();

            List<string> where = new List<string>();
            if (tbSearchFacultyName.Text != "")
                where.Add($"faculty.name = '{tbSearchFacultyName.Text}'");
            if (tbSearchFacultyDepartmentName.Text != "")
                where.Add($"faculty.department_name = '{tbSearchFacultyDepartmentName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT faculty.name, faculty.department_name FROM faculty {String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for(int i =0; i<table.Rows.Count; i++)
                {
                    facultys.Add(new Faculty() { Name = table.Rows[i][0].ToString(), Department = table.Rows[i][1].ToString() });
                }
            
                dgSearchFaculty.ItemsSource = facultys;
                dgSearchFaculty.Columns[0].Header = "Название";
                dgSearchFaculty.Columns[1].Header = "Департамент";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchChairClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(chairGrid);
        }

        private void butSearchChairlick(object sender, RoutedEventArgs e)
        {
            List<Chair> chair = new List<Chair>();

            List<string> where = new List<string>();


            if (tbSearchChairName.Text != "")
                where.Add($"chair.name = '{tbSearchChairName.Text}'");
            if (tbSearchChairFacultyName.Text != "")
                where.Add($"chair.faculty_name = '{tbSearchChairFacultyName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT chair.name, chair.faculty_name FROM chair { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    chair.Add(new Chair() { Name = table.Rows[i][0].ToString(), Faculty = table.Rows[i][1].ToString() });
                }

                dgSearchChair.ItemsSource = chair;
                dgSearchChair.Columns[0].Header = "Название";
                dgSearchChair.Columns[1].Header = "Факультет";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchSpecialityClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(specialityGrid);
        }
        private void butSearchSpecialityClick(object sender, RoutedEventArgs e)
        {
            List<Speciality> speciality = new List<Speciality>();

            List<string> where = new List<string>();

            if (tbSearchSpecialityName.Text != "")
                where.Add($"speciality.name = '{tbSearchSpecialityName.Text}'");
            if (tbSearchSpecialityFacultyName.Text != "")
                where.Add($"speciality.faculty_name = '{tbSearchSpecialityFacultyName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT speciality.name, speciality.faculty_name FROM speciality { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    speciality.Add(new Speciality() { Name = table.Rows[i][0].ToString(), Faculty = table.Rows[i][1].ToString() });
                }

                dgSearchSpecialityGrid.ItemsSource = speciality;
                dgSearchSpecialityGrid.Columns[0].Header = "Название";
                dgSearchSpecialityGrid.Columns[1].Header = "Факультет";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }
        private void butClearSearchDisciplineClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(disciplineGrid);
        }
        private void butSearchDisciplineClick(object sender, RoutedEventArgs e)
        {
            List<Discipline> discipline = new List<Discipline>();

            List<string> where = new List<string>();

            if (tbSearchDisciplineName.Text != "")
                where.Add($"discipline.name = '{tbSearchDisciplineName.Text}'");
            if (tbSearchDisciplineChairName.Text != "")
                where.Add($"discipline.chair_name = '{tbSearchDisciplineChairName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT discipline.name, discipline.chair_name FROM discipline { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    discipline.Add(new Discipline() { Name = table.Rows[i][0].ToString(), Chair = table.Rows[i][1].ToString() });
                }

                dbSearchDiscipline.ItemsSource = discipline;
                dbSearchDiscipline.Columns[0].Header = "Название";
                dbSearchDiscipline.Columns[1].Header = "Кафедра";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchEmployeeClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(employeeGrid);
        }

        private void butSearchEmployeeClick(object sender, RoutedEventArgs e)
        {
            List<Employee> employee = new List<Employee>();

            List<string> where = new List<string>();
            if (tbSearchEmployeeSurname.Text != "")
                where.Add($"employee.surname = '{tbSearchEmployeeSurname.Text}'");
            if (tbSearchEmployeeName.Text != "")
                where.Add($"employee.name = '{tbSearchEmployeeName.Text}'");
            if (tbSearchEmployeePatronymic.Text != "")
                where.Add($"employee.patronymic = '{tbSearchEmployeePatronymic.Text}'");
            if (tbSearchEmployeeINN.Text != "")
                where.Add($"employee.inn = '{tbSearchEmployeeINN.Text}'");
            if (tbSearchEmployeePhoneNumber.Text != "")
                where.Add($"employee.phone_number = '{tbSearchEmployeePhoneNumber.Text}'");
            if (tbSearchEmployeeEmail.Text != "")
                where.Add($"employee.email = '{tbSearchEmployeeEmail.Text}'");
            if (tbSearchEmployeePost.Text != "")
                where.Add($"employee.post = '{tbSearchEmployeePost.Text}'");
            if (tbSearchEmployeeDepartmentName.Text != "")
                where.Add($"employee.department_name = '{tbSearchEmployeeDepartmentName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT employee.id, employee.surname, employee.name, employee.patronymic, employee.inn, employee.phone_number, employee.email, employee.post, employee.salary, employee.department_name, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM employee INNER JOIN passport_data ON passport_data.id = employee.passport_data_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    employee.Add(new Employee()
                    {
                        Id = table.Rows[i][0].ToString(),
                        Surname = table.Rows[i][1].ToString(),
                        Name = table.Rows[i][2].ToString(),
                        Patronymic = table.Rows[i][3].ToString(),
                        Inn = table.Rows[i][4].ToString(),
                        Phone_number = table.Rows[i][5].ToString(),
                        Email = table.Rows[i][6].ToString(),
                        Post = table.Rows[i][7].ToString(),
                        Salary = table.Rows[i][8].ToString(),
                        Department_name = table.Rows[i][9].ToString(),
                        Series = table.Rows[i][10].ToString(),
                        Number = table.Rows[i][11].ToString(),
                        Issue_date = table.Rows[i][12].ToString(),
                        Expiry_date = table.Rows[i][13].ToString(),
                        Issuing_authority = table.Rows[i][14].ToString()
                    });
                }

                dgSearchEmployee.ItemsSource = employee;
                dgSearchEmployee.Columns[0].Header = "Код";
                dgSearchEmployee.Columns[1].Header = "Фамилия";
                dgSearchEmployee.Columns[2].Header = "Имя";
                dgSearchEmployee.Columns[3].Header = "Отчетсво";
                dgSearchEmployee.Columns[4].Header = "ИНН";
                dgSearchEmployee.Columns[5].Header = "Номер телефона";
                dgSearchEmployee.Columns[6].Header = "Email";
                dgSearchEmployee.Columns[7].Header = "Должность";
                dgSearchEmployee.Columns[8].Header = "Зарплата";
                dgSearchEmployee.Columns[9].Header = "Отдел";
                dgSearchEmployee.Columns[10].Header = "Серия паспорта";
                dgSearchEmployee.Columns[11].Header = "Номер паспорта";
                dgSearchEmployee.Columns[12].Header = "Дата выдачи";
                dgSearchEmployee.Columns[13].Header = "Срок действия";
                dgSearchEmployee.Columns[14].Header = "Орган, выдавший паспорт";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchTeacherClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(teacherGrid);
        }

        private void butSearchTeacherClick(object sender, RoutedEventArgs e)
        {
            List<Teacher> teacher = new List<Teacher>();

            List<string> where = new List<string>();

            if (tbSearchTeacherEmployeeId.Text != "")
                where.Add($"teacher.employee_id = '{tbSearchTeacherEmployeeId.Text}'");
            if (tbSearchTeacherChairName.Text != "")
                where.Add($"teacher.chair_name = '{tbSearchTeacherChairName.Text}'");
            if (tbSearchTeacherAcademicRank.Text != "")
                where.Add($"teacher.academic_rank = '{tbSearchTeacherAcademicRank.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT employee.surname, employee.name, employee.patronymic, teacher.chair_name, teacher.academic_rank FROM teacher INNER JOIN employee ON employee.id = teacher.employee_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    teacher.Add(new Teacher(){
                        Surname = table.Rows[i][0].ToString(), 
                        Name = table.Rows[i][1].ToString(), 
                        Patronymic = table.Rows[i][2].ToString(),
                        Chaur_name = table.Rows[i][3].ToString(),
                        Acedemic_rank = table.Rows[i][4].ToString()
                    });
                }

                dgSearchTeacherTable.ItemsSource = teacher;
                dgSearchTeacherTable.Columns[0].Header = "Фамилия";
                dgSearchTeacherTable.Columns[1].Header = "Имя";
                dgSearchTeacherTable.Columns[2].Header = "Отчетсво";
                dgSearchTeacherTable.Columns[3].Header = "Кафедра";
                dgSearchTeacherTable.Columns[4].Header = "Должность";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchGroupClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(groupGrid);
        }

        private void butSearchGroupClick(object sender, RoutedEventArgs e)
        {
            List<Group> group = new List<Group>();

            List<string> where = new List<string>();

            if (tbSearchGroupName.Text != "")
                where.Add($"group.name = '{tbSearchGroupName.Text}'");
            if (tbSearchGroupFacultyName.Text != "")
                where.Add($"group.faculty_name = '{tbSearchGroupFacultyName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT group.name, group.faculty_name FROM `group` { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    group.Add(new Group()
                    {
                        Name = table.Rows[i][0].ToString(),
                        Faculty_name = table.Rows[i][1].ToString(),
                    });
                }

                dgSearchGroup.ItemsSource = group;
                dgSearchGroup.Columns[0].Header = "Название";
                dgSearchGroup.Columns[1].Header = "Факультет";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchStudentClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(studentGrid);
        }

        private void butSearchStudentClick(object sender, RoutedEventArgs e)
        {
            List<Student> student = new List<Student>();

            List<string> where = new List<string>();
            if (tbSearchStudentSurname.Text != "")
                where.Add($"student.surname = '{tbSearchStudentSurname.Text}'");
            if (tbSearchStudentName.Text != "")
                where.Add($"student.name = '{tbSearchStudentName.Text}'");
            if (tbSearchStudentPatronymic.Text != "")
                where.Add($"student.patronymic = '{tbSearchStudentPatronymic.Text}'");
            if (tbSearchStudentINN.Text != "")
                where.Add($"student.inn = '{tbSearchStudentINN.Text}'");
            if (tbSearchStudentPhoneNumber.Text != "")
                where.Add($"student.phone_number = '{tbSearchStudentPhoneNumber.Text}'");
            if (tbSearchStudentEmail.Text != "")
                where.Add($"student.email = '{tbSearchStudentEmail.Text}'");
            if (tbSearchStudentSpecialityName.Text != "")
                where.Add($"student.speciality_name = '{tbSearchStudentSpecialityName.Text}'");
            if (tbSearchStudentChairName.Text != "")
                where.Add($"student.chair_name = '{tbSearchStudentChairName.Text}'");
            if (tbSearchStudentGroupName.Text != "")
                where.Add($"student.group_name = '{tbSearchStudentGroupName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT student.id, student.surname, student.name, student.patronymic, student.speciality_name, student.chair_name, student.group_name, student.start_date, student.end_date, student.education_cost, student.inn, student.phone_number, student.email, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM student INNER JOIN passport_data ON passport_data.id = student.passport_data_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    student.Add(new Student()
                    {
                        Id = table.Rows[i][0].ToString(),
                        Surname = table.Rows[i][1].ToString(),
                        Name = table.Rows[i][2].ToString(),
                        Patronymic = table.Rows[i][3].ToString(),
                        Speciality_name = table.Rows[i][4].ToString(),
                        Chair_name = table.Rows[i][5].ToString(),
                        Group_name = table.Rows[i][6].ToString(),
                        Start_date = table.Rows[i][7].ToString(),
                        End_date = table.Rows[i][8].ToString(),
                        Education_cost = table.Rows[i][9].ToString(),
                        Inn = table.Rows[i][10].ToString(),
                        Phone_number = table.Rows[i][11].ToString(),
                        Email = table.Rows[i][12].ToString(),
                        Series = table.Rows[i][13].ToString(),
                        Number = table.Rows[i][14].ToString(),
                        Issue_date = table.Rows[i][15].ToString(),
                        Expiry_date = table.Rows[i][16].ToString(),
                        Issuing_authority = table.Rows[i][17].ToString()
                    });
                }

                dgSearchStudentTable.ItemsSource = student;
                dgSearchStudentTable.Columns[0].Header = "Код";
                dgSearchStudentTable.Columns[1].Header = "Фамилия";
                dgSearchStudentTable.Columns[2].Header = "Имя";
                dgSearchStudentTable.Columns[3].Header = "Отчетсво";
                dgSearchStudentTable.Columns[4].Header = "Специальность";
                dgSearchStudentTable.Columns[5].Header = "Кафедра";
                dgSearchStudentTable.Columns[6].Header = "Группа";
                dgSearchStudentTable.Columns[7].Header = "Начало обучения";
                dgSearchStudentTable.Columns[8].Header = "Конец обучения";
                dgSearchStudentTable.Columns[9].Header = "Стоимость обучения";
                dgSearchStudentTable.Columns[10].Header = "Инн";
                dgSearchStudentTable.Columns[11].Header = "Телефон";
                dgSearchStudentTable.Columns[12].Header = "Email";
                dgSearchStudentTable.Columns[13].Header = "Серия паспорта";
                dgSearchStudentTable.Columns[14].Header = "Номер паспорта";
                dgSearchStudentTable.Columns[15].Header = "Дата выдачи";
                dgSearchStudentTable.Columns[16].Header = "Срок действия";
                dgSearchStudentTable.Columns[17].Header = "Орган, выдавший паспорт";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchEnrolleeClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(erolleeGrid);
        }

        private void butSearchEnrolleeClick(object sender, RoutedEventArgs e)
        {
            List<Enrollee> enrollee = new List<Enrollee>();

            List<string> where = new List<string>();
            if (tbSearchErolleeSurname.Text != "")
                where.Add($"enrollee.surname = '{tbSearchErolleeSurname.Text}'");
            if (tbSearchErolleeName.Text != "")
                where.Add($"enrollee.name = '{tbSearchErolleeName.Text}'");
            if (tbSearchErolleePatronymic.Text != "")
                where.Add($"enrollee.patronymic = '{tbSearchErolleePatronymic.Text}'");
            if (tbSearchErolleeDocumentType.Text != "")
                where.Add($"enrollee.document_type = '{tbSearchErolleeDocumentType.Text}'");
            if (tbSearchErolleeTotalScore.Text != "")
                where.Add($"enrollee.total_scope = '{tbSearchErolleeTotalScore.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT enrollee.id, enrollee.surname, enrollee.name, enrollee.patronymic, enrollee.document_type, enrollee.total_score, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM enrollee INNER JOIN passport_data ON passport_data.id = enrollee.passport_data_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    enrollee.Add(new Enrollee()
                    {
                        Id = table.Rows[i][0].ToString(),
                        Surname = table.Rows[i][1].ToString(),
                        Name = table.Rows[i][2].ToString(),
                        Patronymic = table.Rows[i][3].ToString(),
                        Document_type = table.Rows[i][4].ToString(),
                        Total_scope = table.Rows[i][5].ToString(),
                        Series = table.Rows[i][6].ToString(),
                        Number = table.Rows[i][7].ToString(),
                        Issue_date = table.Rows[i][8].ToString(),
                        Expiry_date = table.Rows[i][9].ToString(),
                        Issuing_authority = table.Rows[i][10].ToString()
                    });
                }

                dgSearchEnrolleeTable.ItemsSource = enrollee;
                dgSearchEnrolleeTable.Columns[0].Header = "Код";
                dgSearchEnrolleeTable.Columns[1].Header = "Фамилия";
                dgSearchEnrolleeTable.Columns[2].Header = "Имя";
                dgSearchEnrolleeTable.Columns[3].Header = "Отчетсво";
                dgSearchEnrolleeTable.Columns[4].Header = "Тип документа";
                dgSearchEnrolleeTable.Columns[5].Header = "Набрано баллов";
                dgSearchEnrolleeTable.Columns[6].Header = "Серия паспорта";
                dgSearchEnrolleeTable.Columns[7].Header = "Номер паспорта";
                dgSearchEnrolleeTable.Columns[8].Header = "Дата выдачи";
                dgSearchEnrolleeTable.Columns[9].Header = "Срок действия";
                dgSearchEnrolleeTable.Columns[10].Header = "Орган, выдавший паспорт";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butSearchDisciplineTeacherClick(object sender, RoutedEventArgs e)
        {
            List<DisciplineTeacher> disciplineTeacher = new List<DisciplineTeacher>();

            List<string> where = new List<string>();
            if (tbSearchDisciplineTeacherEmployeeId.Text != "")
                where.Add($"discipline_teacher.employee_id = '{tbSearchDisciplineTeacherEmployeeId.Text}'");
            if (tbSearchDisciplineTeacherDisciplineName.Text != "")
                where.Add($"discipline_teacher.discipline_name = '{tbSearchDisciplineTeacherDisciplineName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT employee.surname, employee.name, employee.patronymic, discipline_teacher.discipline_name FROM discipline_teacher INNER JOIN employee ON employee.id = discipline_teacher.employee_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    disciplineTeacher.Add(new DisciplineTeacher()
                    {
                        Surname = table.Rows[i][0].ToString(),
                        Name = table.Rows[i][1].ToString(),
                        Patronymic = table.Rows[i][2].ToString(),
                        Discipline = table.Rows[i][3].ToString(),
                    });
                }

                dgSearchDisciplineTeacherTable.ItemsSource = disciplineTeacher;
                dgSearchDisciplineTeacherTable.Columns[0].Header = "Фамилия";
                dgSearchDisciplineTeacherTable.Columns[1].Header = "Имя";
                dgSearchDisciplineTeacherTable.Columns[2].Header = "Отчетсво";
                dgSearchDisciplineTeacherTable.Columns[3].Header = "Название дисциплины";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearSearchDisciplineTeacherClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(DisciplineTeacherGrid);
        }

        private void butClearSearchDisciplineEnrolleeClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(DisciplineEnrolleeGrid);
        }

        private void butSearchDisciplineEnrolleeClick(object sender, RoutedEventArgs e)
        {
            List<EnrolleeSpeciality> enrolleeSpeciality = new List<EnrolleeSpeciality>();

            List<string> where = new List<string>();
            if (tbSearchDisciplineEnrolleeEnrolleeId.Text != "")
                where.Add($"enrollee_speciality.enrollee_id = '{tbSearchDisciplineEnrolleeEnrolleeId.Text}'");
            if (tbSearchDisciplineEnrolleeSpecialityName.Text != "")
                where.Add($"enrollee_speciality.speciality_name = '{tbSearchDisciplineEnrolleeSpecialityName.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT enrollee.surname, enrollee.name, enrollee.patronymic, enrollee_speciality.speciality_name FROM enrollee_speciality INNER JOIN enrollee ON enrollee.id = enrollee_speciality.enrollee_id { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    enrolleeSpeciality.Add(new EnrolleeSpeciality()
                    {
                        Surname = table.Rows[i][0].ToString(),
                        Name = table.Rows[i][1].ToString(),
                        Patronymic = table.Rows[i][2].ToString(),
                        Speciality = table.Rows[i][3].ToString(),
                    });
                }

                dgSearchDisciplineEnrolleeTable.ItemsSource = enrolleeSpeciality;
                dgSearchDisciplineEnrolleeTable.Columns[0].Header = "Фамилия";
                dgSearchDisciplineEnrolleeTable.Columns[1].Header = "Имя";
                dgSearchDisciplineEnrolleeTable.Columns[2].Header = "Отчетсво";
                dgSearchDisciplineEnrolleeTable.Columns[3].Header = "Специальность";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }

        private void butClearControlSearchClick(object sender, RoutedEventArgs e)
        {
            ClearGrid(gControlSearch);
        }

        private void butControlSearchClick(object sender, RoutedEventArgs e)
        {
            List<UserTable> userTable = new List<UserTable>();

            List<string> where = new List<string>();
            if (tbControlSearchLogin.Text != "")
                where.Add($"user.login = '{tbControlSearchLogin.Text}'");
            if (tbControlSearchSurname.Text != "")
                where.Add($"user.surname = '{tbControlSearchSurname.Text}'");
            if (tbControlSearchName.Text != "")
                where.Add($"user.name = '{tbControlSearchName.Text}'");
            if (tbControlSearchPatronymic.Text != "")
                where.Add($"user.patronymic = '{tbControlSearchPatronymic.Text}'");
            if (tbControlSearchPhoneNumber.Text != "")
                where.Add($"user.phone_number = '{tbControlSearchPhoneNumber.Text}'");
            if (tbControlSearchEmail.Text != "")
                where.Add($"user.email = '{tbControlSearchEmail.Text}'");
            if (tbControlSearchAccessLevel.Text != "---")
                where.Add($"user.access_level = '{tbControlSearchAccessLevel.Text}'");

            if (where.Count() > 0)
            {
                where.Insert(0, "WHERE " + where[0]);
            }

            var table = DBConnection.SelectData($"SELECT user.login, user.surname, user.name, user.patronymic, user.phone_number, user.email, user.access_level FROM user { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    userTable.Add(new UserTable()
                    {
                        Login = table.Rows[i][0].ToString(),
                        Surname = table.Rows[i][1].ToString(),
                        Name = table.Rows[i][2].ToString(),
                        Patronymic = table.Rows[i][3].ToString(),
                        PhoneNumber = table.Rows[i][4].ToString(),
                        Email = table.Rows[i][5].ToString(),
                        AccessLevel = table.Rows[i][6].ToString(),
                    });
                }
                
                dgControlSearch.ItemsSource = userTable;
                dgControlSearch.Columns[0].Header = "Логин";
                dgControlSearch.Columns[1].Header = "Фамилия";
                dgControlSearch.Columns[2].Header = "Имя";
                dgControlSearch.Columns[3].Header = "Отчество";
                dgControlSearch.Columns[4].Header = "Телефон";
                dgControlSearch.Columns[5].Header = "Email";
                dgControlSearch.Columns[6].Header = "Уровень доступа";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
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
