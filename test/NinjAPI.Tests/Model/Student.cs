using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Model
{
    public class Student
    {
        public int ID { get; set; }
        public string LastName { get; set; } = default!;
        public string FirstMidName { get; set; } = default!;
        public DateTime EnrollmentDate { get; set; }

        public virtual ICollection<Enrollment>? Enrollments { get; set; }
    }
}
