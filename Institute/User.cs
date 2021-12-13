using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Institute
{
    internal enum AccessLevel
    {
        Zero,
        Minimal,
        Teacher,
        ChairHead,
        FacultyHead,
        Rector,
        HR,
        AdmissionCommittee,
        Full
    }

    internal static class User
    {
        public static string Login { get; set; }
        public static string Surname { get; set; }
        public static string Name { get; set; }
        public static string Patronymic { get; set; }
        public static string PhoneNumber { get; set; }
        public static string Email { get; set; }
        public static AccessLevel AccessLevel { get; set; }

        public static void SetAccessLevel()
        {
            ;
        }
    }
}
