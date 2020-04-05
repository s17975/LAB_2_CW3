using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public class my_dbService : IDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s17975;Integrated Security=True";

        public IEnumerable<Student> GetStudents()
        {
            var list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select IndexNumber, FirstName, LastName, BirthDate, Studies.Name, Semester, StartDate from s17975.dbo.Student LEFT JOIN s17975.dbo.Enrollment ON s17975.dbo.Student.IdEnrollment = s17975.dbo.Enrollment.IdEnrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Enrollment.IdStudy = s17975.dbo.Studies.IdStudy;";
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = dr["BirthDate"].ToString();
                    list.Add(st);
                }
            }
            return list;
        }
        public IEnumerable<Student> GetStudent(string index)
        {

            var list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select IndexNumber, FirstName, LastName, BirthDate, Studies.Name, Semester, StartDate from s17975.dbo.Student LEFT JOIN s17975.dbo.Enrollment ON s17975.dbo.Student.IdEnrollment = s17975.dbo.Enrollment.IdEnrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Enrollment.IdStudy = s17975.dbo.Studies.IdStudy WHERE IndexNumber='" + @index + "';";
                com.Parameters.AddWithValue("IndexNumber", index);
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = dr["BirthDate"].ToString();
                    list.Add(st);
                }
            }
            return list;
        }
        public int DeleteStudent(string index)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                int retValue = 0;
                com.Connection = con;
                com.CommandText = "DELETE FROM s17975.dbo.Student WHERE s17975.dbo.Student.IndexNumber = '" + @index + "';";
                com.Parameters.AddWithValue("IndexNumber", index);
                con.Open();
                retValue = com.ExecuteNonQuery();
                return retValue;
            }
            
        }

    }
}
