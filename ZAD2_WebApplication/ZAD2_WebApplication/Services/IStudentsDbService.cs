using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public interface IStudentsDbService
    {
        IEnumerable<Student> GetStudents();
        IEnumerable<Student> GetStudent(string indexNumber);
        int DeleteStudent(string indexNumber);
        Response_Enrollment EnrollStudent(Request_EnrollStudent request);
        Response_Enrollment PromoteStudents(int Semester, string Studies);
    }
}
