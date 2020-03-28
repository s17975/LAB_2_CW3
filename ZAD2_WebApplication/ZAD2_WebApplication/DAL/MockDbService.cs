using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;

        static MockDbService()
        {
            _students = new List<Student>
            {
                new Student{IdStudent=1, FirstName="Robert",LastName="Robertowski"},
                new Student{IdStudent=2, FirstName="Marek",LastName="Pas"},
                new Student{IdStudent=3, FirstName="Marcin",LastName="Kot"}
            };
        }
        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
