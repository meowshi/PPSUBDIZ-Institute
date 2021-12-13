using System;
using System.Collections.Generic;
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
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
            string passportQuery = $"insert into passport_data ({PassportColumnsStr}) values ({passportValues})";
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

        private void butAddTeacherDisc_Click(object sender, RoutedEventArgs e)
        {
            Add("discipline_teacher", gAddTeacherDisc);
        }

        private void butClearAddTeacherDisc_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddTeacherDisc);
        }

        private void butAddEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            Add("enrollee_speciality", gAddEnrolleeSpec);
        }

        private void butClearAddEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gAddEnrolleeSpec);
        }

        private void Change(Grid grid, string table, string[] columns, string keyColumnName, string key, bool canDelete = true)
        {
            bool isOtherFieldsNotEmpty = IsFieldsNotEmpty(grid);
            if (!isOtherFieldsNotEmpty)
            {
                if (canDelete)
                {
                    string query = $"delete from {table} where {keyColumnName} = '{key}'";
                    if (DBConnection.ChangeData(query))
                    {
                        MessageBox.Show("Удаление прошло успешно", "Ура!");
                    }
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
                    MessageBox.Show("Изменение прошло успешно!", "Ура!");
                }
            }
        }

        private void butChangeFaculty_Click(object sender, RoutedEventArgs e)
        {
            if (tbChangeFacultyName.Text == "")
            {
                MessageBox.Show("Вы не заполнили ключевое поле!", "Внимание!");
                return;
            }

            Change(gChangeFacultyOther, "faculty", FacultyFields, "name", tbChangeFacultyName.Text);
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

            Change(gChangeChairOther, "chair", ChairFields, "name", tbChangeChairName.Text);
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

            Change(gChangeSpecialityOther, "speciality", SpecialityFields, "name", tbChangeSpecialityName.Text);
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

            Change(gChangeDisciplineOther, "discipline", DisciplineFields, "name", tbChangeDisciplineName.Text);
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
            string passportDataId = employeeTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeEmployeePassport) || IsFieldsNotEmpty(gChangeEmployeeOther));

            Change(gChangeEmployeeOther, "employee", EmployeeFields, "id", tbChangeEmployeeId.Text, canDelete);
            Change(gChangeEmployeePassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
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

            Change(gChangeTeacherOther, "teacher", TeacherFields, "employee_id", tbChangeTeacherEmployeeId.Text);
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

            Change(gChangeGroupOther, "`group`", GroupFields, "name", tbChangeGroupName.Text);
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
            string passportDataId = studentTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeStudentPassport) || IsFieldsNotEmpty(gChangeStudentOther));

            Change(gChangeStudentOther, "student", StudentFields, "id", tbChangeStudentId.Text, canDelete);
            Change(gChangeStudentPassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
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
            string passportDataId = enrolleeTable.Rows[0][0].ToString();

            bool canDelete = !(IsFieldsNotEmpty(gChangeEnrolleePassport) || IsFieldsNotEmpty(gChangeEnrolleeOther));

            Change(gChangeEnrolleeOther, "enrollee", EnrolleeFields, "id", tbChangeEnrolleeId.Text, canDelete);
            Change(gChangeEnrolleePassport, "passport_data", PassportColumns, "id", passportDataId, canDelete);
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
            if (!IsFieldsFilled(gChangeDiscTeacher))
            {
                MessageBox.Show("Вы не заполнили поля!", "Внимание!");
                return;
            }

            string query = $"delete from discipline_teacher where discipline_name = '{tbCangeTeacherDiscDiscName.Text}' and employee_id = '{tbChangeTeacherDiscTeacherId.Text}'";
            if (DBConnection.ChangeData(query))
            {
                MessageBox.Show("Данные успешно удалены!", "Ура!");
            }
        }

        private void butClearChangeDiscTeacher_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeDiscTeacher);
        }

        private void butChangeEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldsFilled(gChangeEnrolleeSpec))
            {
                MessageBox.Show("Вы не заполнили поля!", "Внимание!");
                return;
            }

            string query = $"delete from enrollee_speciality where enrollee_id = '{tbChangeEnrolleeSpecEnrolleeId.Text}' and speciality_name = '{tbChangeEnrolleeSpecSpecName.Text}'";
            if (DBConnection.ChangeData(query))
            {
                MessageBox.Show("Данные успешно удалены!", "Ура!");
            }
        }

        private void butClearChangeEnrolleeSpec_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid(gChangeEnrolleeSpec);
        }

        private void butControlAdd_Click(object sender, RoutedEventArgs e)
        {
            Add("user", gControlAdd);
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

            Change(gControlChangeOther, "user", UserFields, "login", tbControlChangeLogin.Text);
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
            bool isKeyEmpty = tbSearchDepartmentName.Text == "";

            if (isKeyEmpty)
            {
                MessageBox.Show("Введите наименование", "Внимание!");
                return;
            }
            var table = DBConnection.SelectData("SELECT department.name, count(employee.department_name) FROM department INNER JOIN employee ON employee.department_name = department.name WHERE department.name = 'Учебный отдел' GROUP BY employee.department_name; ");

            departmentSearchResaltName.Content = table.Rows[0][0];
            departmentSearchResaltStuff.Content = table.Rows[0][1];

            departmentSearchResalt.Visibility = Visibility.Visible;
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
            if (tbSearchFacultyName.Text == "" && tbSearchFacultyDepartmentName.Text == "")
            {
                MessageBox.Show("Введите наименование или отдел", "Внимание!");
                return;
            }
            List<Faculty> facultys = new List<Faculty>();

            List<string> where = new List<string>();
            if (tbSearchFacultyName.Text != "")
                where.Add($"faculty.name = '{tbSearchFacultyName.Text}'");
            if (tbSearchFacultyDepartmentName.Text != "")
                where.Add($"faculty.department_name = '{tbSearchFacultyDepartmentName.Text}'");

            var table = DBConnection.SelectData($"SELECT faculty.name, faculty.department_name FROM faculty WHERE {String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchChairName.Text == "" && tbSearchChairFacultyName.Text == "")
            {
                MessageBox.Show("Введите наименование кафедры или факультета", "Внимание!");
                return;
            }
            List<Chair> chair = new List<Chair>();

            List<string> where = new List<string>();
            if (tbSearchChairName.Text != "")
                where.Add($"chair.name = '{tbSearchChairName.Text}'");
            if (tbSearchChairFacultyName.Text != "")
                where.Add($"chair.faculty_name = '{tbSearchChairFacultyName.Text}'");

            var table = DBConnection.SelectData($"SELECT chair.name, chair.faculty_name FROM chair WHERE { String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchSpecialityName.Text == "" && tbSearchSpecialityFacultyName.Text == "")
            {
                MessageBox.Show("Введите наименование кафедры или факультета", "Внимание!");
                return;
            }
            List<Speciality> speciality = new List<Speciality>();

            List<string> where = new List<string>();
            if (tbSearchSpecialityName.Text != "")
                where.Add($"speciality.name = '{tbSearchSpecialityName.Text}'");
            if (tbSearchSpecialityFacultyName.Text != "")
                where.Add($"speciality.faculty_name = '{tbSearchSpecialityFacultyName.Text}'");

            var table = DBConnection.SelectData($"SELECT speciality.name, speciality.faculty_name FROM speciality WHERE { String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchDisciplineName.Text == "" && tbSearchDisciplineChairName.Text == "")
            {
                MessageBox.Show("Введите наименование кафедры или дисциплины", "Внимание!");
                return;
            }
            List<Discipline> discipline = new List<Discipline>();

            List<string> where = new List<string>();
            if (tbSearchDisciplineName.Text != "")
                where.Add($"discipline.name = '{tbSearchDisciplineName.Text}'");
            if (tbSearchDisciplineChairName.Text != "")
                where.Add($"discipline.chair_name = '{tbSearchDisciplineChairName.Text}'");

            var table = DBConnection.SelectData($"SELECT discipline.name, discipline.chair_name FROM discipline WHERE { String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchEmployeeSurname.Text == "" && tbSearchEmployeeName.Text == "" &&
                tbSearchEmployeePatronymic.Text == "" && tbSearchEmployeeINN.Text == "" &&
                tbSearchEmployeePost.Text == "" && tbSearchEmployeeSalary.Text == "" &&
                tbSearchEmployeeDepartmentName.Text == "" && tbSearchEmployeeEmail.Text == "" &&
                tbSearchEmployeePhoneNumber.Text == "")
            {
                MessageBox.Show("Заполните любое поле", "Внимание!");
                return;
            }
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
            if (tbSearchEmployeeSalary.Text != "")
                where.Add($"employee.salary = '{tbSearchEmployeeSalary.Text}'");
            if (tbSearchEmployeeDepartmentName.Text != "")
                where.Add($"employee.department_name = '{tbSearchEmployeeDepartmentName.Text}'");

            var table = DBConnection.SelectData($"SELECT employee.surname, employee.name, employee.patronymic, employee.inn, employee.phone_number, employee.email, employee.post, employee.salary, employee.department_name, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM employee INNER JOIN passport_data ON passport_data.id = employee.passport_data_id WHERE { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    employee.Add(new Employee()
                    {
                        Surname = table.Rows[i][0].ToString(),
                        Name = table.Rows[i][1].ToString(),
                        Patronymic = table.Rows[i][2].ToString(),
                        Inn = table.Rows[i][3].ToString(),
                        Phone_number = table.Rows[i][4].ToString(),
                        Email = table.Rows[i][5].ToString(),
                        Post = table.Rows[i][6].ToString(),
                        Salary = table.Rows[i][7].ToString(),
                        Department_name = table.Rows[i][8].ToString(),
                        Series = table.Rows[i][9].ToString(),
                        Number = table.Rows[i][10].ToString(),
                        Issue_date = table.Rows[i][11].ToString(),
                        Expiry_date = table.Rows[i][12].ToString(),
                        Issuing_authority = table.Rows[i][13].ToString()
                    });
                }

                dgSearchEmployee.ItemsSource = employee;
                dgSearchEmployee.Columns[0].Header = "Фамилия";
                dgSearchEmployee.Columns[1].Header = "Имя";
                dgSearchEmployee.Columns[2].Header = "Отчетсво";
                dgSearchEmployee.Columns[3].Header = "ИНН";
                dgSearchEmployee.Columns[4].Header = "Номер телефона";
                dgSearchEmployee.Columns[5].Header = "Email";
                dgSearchEmployee.Columns[6].Header = "Должность";
                dgSearchEmployee.Columns[7].Header = "Зарплата";
                dgSearchEmployee.Columns[8].Header = "Отдел";
                dgSearchEmployee.Columns[9].Header = "Серия паспорта";
                dgSearchEmployee.Columns[10].Header = "Номер паспорта";
                dgSearchEmployee.Columns[11].Header = "Дата выдачи";
                dgSearchEmployee.Columns[12].Header = "Срок действия";
                dgSearchEmployee.Columns[13].Header = "Орган, выдавший паспорт";
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
            if (tbSearchTeacherEmployeeId.Text == "" && tbSearchTeacherChairName.Text == "" &&
                tbSearchTeacherAcademicRank.Text == "")
            {
                MessageBox.Show("Заполните любое поле", "Внимание!");
                return;
            }
            List<Teacher> teacher = new List<Teacher>();

            List<string> where = new List<string>();

            if (tbSearchTeacherEmployeeId.Text != "")
                where.Add($"teacher.employee_id = '{tbSearchTeacherEmployeeId.Text}'");
            if (tbSearchTeacherChairName.Text != "")
                where.Add($"teacher.chair_name = '{tbSearchTeacherChairName.Text}'");
            if (tbSearchTeacherAcademicRank.Text != "")
                where.Add($"teacher.academic_rank = '{tbSearchTeacherAcademicRank.Text}'");

            var table = DBConnection.SelectData($"SELECT employee.surname, employee.name, employee.patronymic, teacher.chair_name, teacher.academic_rank FROM teacher INNER JOIN employee ON employee.id = teacher.employee_id WHERE { String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchGroupName.Text == "" && tbSearchGroupFacultyName.Text == "")
            {
                MessageBox.Show("Заполните любое поле", "Внимание!");
                return;
            }
            List<Group> group = new List<Group>();

            List<string> where = new List<string>();

            if (tbSearchGroupName.Text != "")
                where.Add($"group.name = '{tbSearchGroupName.Text}'");
            if (tbSearchGroupFacultyName.Text != "")
                where.Add($"group.faculty_name = '{tbSearchGroupFacultyName.Text}'");

            var table = DBConnection.SelectData($"SELECT group.name, group.faculty_name FROM `group` WHERE { String.Join(" AND ", where.ToArray())};");
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
            if (tbSearchStudentSurname.Text == "" && tbSearchStudentName.Text == "" &&
                tbSearchStudentPatronymic.Text == "" && tbSearchStudentINN.Text == "" &&
                tbSearchStudentPhoneNumber.Text == "" && tbSearchStudentEmail.Text == "" &&
                tbSearchStudentSpecialityName.Text == "" && tbSearchStudentChairName.Text == "" &&
                tbSearchStudentGroupName.Text == "")
            {
                MessageBox.Show("Заполните любое поле", "Внимание!");
                return;
            }
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

            var table = DBConnection.SelectData($"SELECT student.surname, student.name, student.patronymic, student.speciality_name, student.chair_name, student.group_name, student.start_date, student.end_date, student.education_cost, student.inn, student.phone_number, student.email, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM student INNER JOIN passport_data ON passport_data.id = student.passport_data_id WHERE { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    student.Add(new Student()
                    {
                        Surname = table.Rows[i][0].ToString(),
                        Name = table.Rows[i][1].ToString(),
                        Patronymic = table.Rows[i][2].ToString(),
                        Speciality_name = table.Rows[i][3].ToString(),
                        Chair_name = table.Rows[i][4].ToString(),
                        Group_name = table.Rows[i][5].ToString(),
                        Start_date = table.Rows[i][6].ToString(),
                        End_date = table.Rows[i][7].ToString(),
                        Education_cost = table.Rows[i][8].ToString(),
                        Inn = table.Rows[i][9].ToString(),
                        Phone_number = table.Rows[i][10].ToString(),
                        Email = table.Rows[i][11].ToString(),
                        Series = table.Rows[i][12].ToString(),
                        Number = table.Rows[i][13].ToString(),
                        Issue_date = table.Rows[i][14].ToString(),
                        Expiry_date = table.Rows[i][15].ToString(),
                        Issuing_authority = table.Rows[i][16].ToString()
                    });
                }

                dgSearchStudentTable.ItemsSource = student;
                dgSearchStudentTable.Columns[0].Header = "Фамилия";
                dgSearchStudentTable.Columns[1].Header = "Имя";
                dgSearchStudentTable.Columns[2].Header = "Отчетсво";
                dgSearchStudentTable.Columns[3].Header = "Специальность";
                dgSearchStudentTable.Columns[4].Header = "Кафедра";
                dgSearchStudentTable.Columns[5].Header = "Группа";
                dgSearchStudentTable.Columns[6].Header = "Начало обучения";
                dgSearchStudentTable.Columns[7].Header = "Конец обучения";
                dgSearchStudentTable.Columns[8].Header = "Стоимость обучения";
                dgSearchStudentTable.Columns[9].Header = "Инн";
                dgSearchStudentTable.Columns[10].Header = "Телефон";
                dgSearchStudentTable.Columns[11].Header = "Email";
                dgSearchStudentTable.Columns[12].Header = "Серия паспорта";
                dgSearchStudentTable.Columns[13].Header = "Номер паспорта";
                dgSearchStudentTable.Columns[14].Header = "Дата выдачи";
                dgSearchStudentTable.Columns[15].Header = "Срок действия";
                dgSearchStudentTable.Columns[16].Header = "Орган, выдавший паспорт";
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
            if (tbSearchErolleeSurname.Text == "" && tbSearchErolleeName.Text == "" &&
                tbSearchErolleePatronymic.Text == "" && tbSearchErolleeDocumentType.Text == "" &&
                tbSearchErolleeTotalScore.Text == "")
            {
                MessageBox.Show("Заполните любое поле", "Внимание!");
                return;
            }
            List<Enrollee> enrollee = new List<Enrollee>();

            List<string> where = new List<string>();
            if (tbSearchErolleeSurname.Text != "")
                where.Add($"enrollee.surname = '{tbSearchErolleeSurname.Text}'");
            if (tbSearchErolleeName.Text != "")
                where.Add($"enrollee.name = '{tbSearchErolleeName.Text}'");
            if (tbSearchErolleePatronymic.Text != "")
                where.Add($"enrollee.patronymic = '{tbSearchErolleePatronymic.Text}'");
            if (tbSearchErolleeDocumentType.Text != "")
                where.Add($"enrollee.docunebt_type = '{tbSearchErolleeDocumentType.Text}'");
            if (tbSearchErolleeTotalScore.Text != "")
                where.Add($"enrollee.total_scope = '{tbSearchErolleeTotalScore.Text}'");

            var table = DBConnection.SelectData($"SELECT enrollee.surname, enrollee.name, enrollee.patronymic, enrollee.document_type, enrollee.total_score, passport_data.series, passport_data.number, passport_data.issue_date, passport_data.expiry_date, passport_data.issuing_authority FROM enrollee INNER JOIN passport_data ON passport_data.id = enrollee.passport_data_id WHERE { String.Join(" AND ", where.ToArray())};");
            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    enrollee.Add(new Enrollee()
                    {
                        Surname = table.Rows[i][0].ToString(),
                        Name = table.Rows[i][1].ToString(),
                        Patronymic = table.Rows[i][2].ToString(),
                        Document_type = table.Rows[i][3].ToString(),
                        Total_scope = table.Rows[i][4].ToString(),
                        Series = table.Rows[i][5].ToString(),
                        Number = table.Rows[i][6].ToString(),
                        Issue_date = table.Rows[i][7].ToString(),
                        Expiry_date = table.Rows[i][8].ToString(),
                        Issuing_authority = table.Rows[i][9].ToString()
                    });
                }

                dgSearchEnrolleeTable.ItemsSource = enrollee;
                dgSearchEnrolleeTable.Columns[0].Header = "Фамилия";
                dgSearchEnrolleeTable.Columns[1].Header = "Имя";
                dgSearchEnrolleeTable.Columns[2].Header = "Отчетсво";
                dgSearchEnrolleeTable.Columns[3].Header = "Тип документа";
                dgSearchEnrolleeTable.Columns[4].Header = "Набрано баллов";
                dgSearchEnrolleeTable.Columns[5].Header = "Серия паспорта";
                dgSearchEnrolleeTable.Columns[6].Header = "Номер паспорта";
                dgSearchEnrolleeTable.Columns[7].Header = "Дата выдачи";
                dgSearchEnrolleeTable.Columns[8].Header = "Срок действия";
                dgSearchEnrolleeTable.Columns[9].Header = "Орган, выдавший паспорт";
            }
            else
            {
                MessageBox.Show("Ничего не найдено");
            }
        }
    }
}
