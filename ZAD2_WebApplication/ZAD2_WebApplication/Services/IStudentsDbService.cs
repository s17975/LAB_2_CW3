using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public interface IStudentsDbService
    {
        Response_StudentAuthentication AuthenticateStudent(Request_StudentAuthentication request);
        Response_AuthorizationRoles GetAuthorization(string IndexNumber);
        string GetRefreshTokenOwner(string token);
        void SetRefreshToken(Guid token, string indexNumber);
        string CheckIndexNumberInDB(string indexNumber);
        IEnumerable<Student> GetStudents();
        IEnumerable<Student> GetStudent(string indexNumber);
        int DeleteStudent(string indexNumber);
        Response_Enrollment EnrollStudent(Request_EnrollStudent request);
        Response_Enrollment PromoteStudents(int Semester, string Studies);
    }
}
