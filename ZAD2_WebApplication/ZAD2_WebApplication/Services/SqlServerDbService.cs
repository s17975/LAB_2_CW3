
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s17975;Integrated Security=True";

        public IEnumerable<Student> GetStudents()
        {
            var list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select IndexNumber, FirstName, LastName, BirthDate, Studies.Name, Semester, StartDate, Enrollment.IdEnrollment from s17975.dbo.Student LEFT JOIN s17975.dbo.Enrollment ON s17975.dbo.Student.IdEnrollment = s17975.dbo.Enrollment.IdEnrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Enrollment.IdStudy = s17975.dbo.Studies.IdStudy;";
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var student = new Student();
                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.BirthDate = DateTime.ParseExact(dr["BirthDate"].ToString(), "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                    student.IDEnrollment = (int) dr["IdEnrollment"];
                    list.Add(student);
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
                com.CommandText = "select IndexNumber, FirstName, LastName, BirthDate, Studies.Name, Semester, StartDate, Enrollment.IdEnrollment from s17975.dbo.Student LEFT JOIN s17975.dbo.Enrollment ON s17975.dbo.Student.IdEnrollment = s17975.dbo.Enrollment.IdEnrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Enrollment.IdStudy = s17975.dbo.Studies.IdStudy WHERE IndexNumber='" + @index + "';";
                com.Parameters.AddWithValue("IndexNumber", index);
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var student = new Student();
                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.BirthDate = DateTime.ParseExact(dr["BirthDate"].ToString(), "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                    student.IDEnrollment = (int)dr["IdEnrollment"];
                    list.Add(student);
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

        public Response_Enrollment EnrollStudent(Request_EnrollStudent request)
        {
            Response_Enrollment response = new Response_Enrollment();
            var studies = request.Studies;
            var student = new Student();
            int idstudies = 0;
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.IndexNumber = request.IndexNumber;
            student.BirthDate = request.BirthDate;
            
            using (var con = new SqlConnection(ConString))
            using (var com = con.CreateCommand())
            {
                con.Open();

                //1. Czy studia istnieja?
                com.CommandText = "SELECT IdStudy FROM s17975.dbo.Studies WHERE s17975.dbo.Studies.Name = '" + studies + "';";
                using (var dr = com.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        throw new Exception("Wybrene studia nie istnieją !");
                    }
                    idstudies = (int)dr["IdStudy"];
                }

                //2. Obecne ID Enrollment?
                com.CommandText = "SELECT MAX(IdEnrollment) as IdEnrollment from s17975.dbo.Enrollment;";
                using (var dr = com.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        student.IDEnrollment = 1;
                    }
                    if (string.IsNullOrEmpty(dr["IdEnrollment"].ToString()))
                    {
                        student.IDEnrollment = 1;
                    } else
                    {
                        student.IDEnrollment = (int)dr["IdEnrollment"] + 1;
                    }
                }

                //3. IndexNumer unikalny?
                com.CommandText = "SELECT IndexNumber from s17975.dbo.Student WHERE s17975.dbo.Student.IndexNumber = '" + student.IndexNumber + "';";
                using (var dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        throw new Exception("IndexNumber już istnieje !");
                    }
                }

                //4. Wpis już istnieje?
                com.CommandText = "SELECT IdEnrollment from s17975.dbo.Enrollment WHERE s17975.dbo.Enrollment.IdStudy='" + idstudies + "' AND s17975.dbo.Enrollment.Semester='1';";
                Boolean exists_Enrollment = false;
                using (var dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        Console.WriteLine("Wpis już istnieje");
                        student.IDEnrollment = student.IDEnrollment = (int)dr["IdEnrollment"];
                        exists_Enrollment = true;
                    }
                }
                SqlTransaction transaction;
                transaction = con.BeginTransaction("INSERTY");
                com.Connection = con;
                com.Transaction = transaction;
                try
                {
                    //5. Dodanie enrollment
                    if(!exists_Enrollment)
                    {
                        com.CommandText = "INSERT INTO s17975.dbo.Enrollment VALUES(" + student.IDEnrollment + ", 1 , " + idstudies + ", '" + DateTime.Now.ToString("dd.MM.yyyy") + "');";
                        com.ExecuteNonQuery();
                    }
                    //6. Dodanie studenta
                    com.CommandText = "INSERT INTO s17975.dbo.Student VALUES('" + student.IndexNumber + "', '" + student.FirstName + "', '" + student.LastName + "', '" + student.BirthDate.ToString("yyyy-MM-dd") + "' , " +student.IDEnrollment+");";
                    com.ExecuteNonQuery();
                    transaction.Commit();
                    response.semester = 1;
                }
                catch (SqlException exc)
                {
                    Console.WriteLine("Blad podczas wykonywania polecenia SQL : "+ exc.Message);
                    transaction.Rollback();
                    response.semester = 0;
                }
            }
            return response;
        }

        public void PromoteStudents(int semester, string studies)
        {
            Console.WriteLine("SEM :" + semester.ToString());
        }
    }
}
