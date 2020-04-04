using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public interface IDbService
    {
        IEnumerable<Student> GetStudents();
        IEnumerable<Student> GetStudent(string indexNumber);
    }
}
