using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NinjAPI.UseExample.Models
{
    public class Student
    {
        [Key]
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstMidName { get; set; }
        public DateTime EnrollmentDate { get; set; }

        [JsonIgnore]
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}