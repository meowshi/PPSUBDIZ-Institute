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
    }
}
