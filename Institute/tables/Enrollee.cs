using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Institute.tables
{
    internal class Enrollee
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Document_type { get; set; }
        public string Total_scope { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string Issue_date { get; set; }
        public string Expiry_date { get; set; }
        public string Issuing_authority { get; set; }
    }
}
