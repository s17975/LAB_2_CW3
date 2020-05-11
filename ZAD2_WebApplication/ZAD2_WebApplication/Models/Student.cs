using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZAD2_WebApplication.Models
{
    public class Student
    {
        public int IDEnrollment { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IndexNumber { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
