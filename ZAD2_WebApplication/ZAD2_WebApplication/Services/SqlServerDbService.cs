
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.DAL
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s17975;Integrated Security=True";

        public Response_StudentAuthentication AuthenticateStudent(Request_StudentAuthentication request) 
        {
            Response_StudentAuthentication response = new Response_StudentAuthentication();
            IEnumerable<Student> student = GetStudent(request.IndexNumber);
            if (student == null || student.Count() == 0)
            {
                throw new Exception("Brak autoryzacji, podane konto studenta nie istnieje !");
            }
            response.IndexNumber = student.FirstOrDefault().IndexNumber;
            response.Password = student.FirstOrDefault().Password;
            response.Salt = student.FirstOrDefault().Salt;
            return response;
        }

        public Response_AuthorizationRoles GetAuthorization(string IndexNumber) 
        {
            Response_AuthorizationRoles response = new Response_AuthorizationRoles();

            var roles_list = new List<string>();
            string role;
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT s17975.dbo.Roles.DB_Role from s17975.dbo.Roles WHERE s17975.dbo.Roles.IndexNumber = '"+ IndexNumber + "';";
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    role = dr["DB_Role"].ToString();
                    roles_list.Add(role);
                }
            }
            response.DB_Roles = roles_list;
            if(response == null || response.DB_Roles.Count() == 0)
            {
                throw new Exception("Nie masz uprawnień do wyświetlenia wyniku !");
            }
            return response;
        }

        public string GetRefreshTokenOwner(string token) 
        {
            string IndexNumber = "brak";
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT s17975.dbo.IndexNumber from s17975.dbo.RefreshTokens WHERE s17975.dbo.RefreshTokens.Token = '" + token + "';";
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    IndexNumber = dr["IndexNumber"].ToString();
                }
            }
            if (IndexNumber == null || IndexNumber == "brak")
            {
                throw new Exception("Token nie zarejestrowany w bazie danych !");
            }
            return IndexNumber;
        }

        public void SetRefreshToken(Guid token, string indexNumber)
        {
            string OldToken = "brak";
            using (var con = new SqlConnection(ConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                com.CommandText = "SELECT s17975.dbo.RefreshTokens.Token from s17975.dbo.RefreshTokens WHERE s17975.dbo.RefreshTokens.IndexNumber = '" + indexNumber + "';";

                using (var dr = com.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OldToken = dr["Token"].ToString();
                    }
                }
                    if (OldToken == null || OldToken == "brak")
                    {
                    SqlTransaction transaction;
                    transaction = con.BeginTransaction("INSERT");
                    com.Connection = con;
                    com.Transaction = transaction;
                    try
                    {
                        // Dodanie refreshToken dla unikatowego numeru studenta/pracownika (przy założeniu że każdy pracownik ma swój 'numer indexu')
                        com.CommandText = "INSERT INTO s17975.dbo.RefreshTokens VALUES('" + indexNumber + "','"+ token.ToString() + "');";
                        com.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (SqlException exc)
                    {
                        Console.WriteLine("Blad podczas wykonywania polecenia SQL : " + exc.Message);
                        transaction.Rollback();
                    }
                } else
                {
                    SqlTransaction transaction;
                    transaction = con.BeginTransaction("UPDATE");
                    com.Connection = con;
                    com.Transaction = transaction;
                    try
                    {
                        // Update refreshToken dla unikatowego numeru studenta/pracownika (przy założeniu że każdy pracownik ma swój 'numer indexu')
                        com.CommandText = "UPDATE s17975.dbo.RefreshTokens SET s17975.dbo.RefreshTokens.Token='" + token.ToString() + "' WHERE s17975.dbo.RefreshTokens.IndexNumber='"+indexNumber+"';";
                        com.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (SqlException exc)
                    {
                        Console.WriteLine("Blad podczas wykonywania polecenia SQL : " + exc.Message);
                        transaction.Rollback();
                    }
                }
            }
        }

        public string CheckIndexNumberInDB(string IndexNumber)
        {
            string response = "brak";
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT s17975.dbo.Student.IndexNumber FROM s17975.dbo.Student WHERE s17975.dbo.Student.IndexNumber='"+ @IndexNumber + "';";
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);
                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    response = dr["IndexNumber"].ToString();
                }
            }
            return response;
    }

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

        public IEnumerable<Student> GetStudent(string IndexNumber)
        {
            var list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select IndexNumber, FirstName, LastName, BirthDate, Password, Salt, Studies.Name, Semester, StartDate, Enrollment.IdEnrollment from s17975.dbo.Student LEFT JOIN s17975.dbo.Enrollment ON s17975.dbo.Student.IdEnrollment = s17975.dbo.Enrollment.IdEnrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Enrollment.IdStudy = s17975.dbo.Studies.IdStudy WHERE IndexNumber='" + @IndexNumber + "';";
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);
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
                    student.Password = dr["Password"].ToString();
                    student.Salt = dr["Salt"].ToString();
                    list.Add(student);
                }
            }
            return list;
        }

        public int DeleteStudent(string IndexNumber)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                int retValue = 0;
                com.Connection = con;
                com.CommandText = "DELETE FROM s17975.dbo.Student WHERE s17975.dbo.Student.IndexNumber = '" + @IndexNumber + "';";
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);
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
                    //5. Dodanie enrollment jeżeli nie istnieje
                    if(!exists_Enrollment)
                    {
                        com.CommandText = "INSERT INTO s17975.dbo.Enrollment VALUES(" + student.IDEnrollment + ", 1 , " + idstudies + ", '" + DateTime.Now.ToString("dd.MM.yyyy") + "');";
                        com.ExecuteNonQuery();
                    }
                    //6. Dodanie studenta
                    com.CommandText = "INSERT INTO s17975.dbo.Student VALUES('" + student.IndexNumber + "', '" + student.FirstName + "', '" + student.LastName + "', '" + student.BirthDate.ToString("yyyy-MM-dd") + "' , " +student.IDEnrollment+");";
                    com.ExecuteNonQuery();
                    transaction.Commit();
                    response.Semester = 1;
                    response.IdEnrollment = student.IDEnrollment;
                    response.IdStudy = idstudies;
                    response.StartDate = DateTime.Now;
                }
                catch (SqlException exc)
                {
                    Console.WriteLine("Blad podczas wykonywania polecenia SQL : "+ exc.Message);
                    transaction.Rollback();
                    response.Semester = 0;
                }
            }
            return response;
        }

        public Response_Enrollment PromoteStudents(int Semester, string Studies)
        {
            Response_Enrollment response = new Response_Enrollment();
            using (var con = new SqlConnection(ConString))
            using (var com = con.CreateCommand())
            {
                con.Open();

                //1. Czy studia istnieja?
                com.CommandText = "select s17975.dbo.Enrollment.IdEnrollment,s17975.dbo.Enrollment.Semester, s17975.dbo.Enrollment.IdStudy, s17975.dbo.Enrollment.StartDate  from s17975.dbo.Enrollment LEFT JOIN s17975.dbo.Studies ON s17975.dbo.Studies.IdStudy = s17975.dbo.Enrollment.IdStudy WHERE s17975.dbo.Studies.Name='" + @Studies + "' AND s17975.dbo.Enrollment.Semester="+ @Semester + ";";
                com.Parameters.AddWithValue("Name", Studies);
                com.Parameters.AddWithValue("Semester", Semester);
                using (var dr = com.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        throw new Exception("Wybrene studia nie istnieją !");
                    }
                }
                //2. Studenci LevelUp - wywołanie procedury składniowej
                com.CommandText = "EXEC APBD_PromoteStudents "+ Semester + ", " + Studies;
                using (var dr = com.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        throw new Exception("Procedura składniowa nie zwróciła wyniku");
                    }
                    response.IdEnrollment = (int)dr["IdEnrollment"];
                    response.Semester = (int)dr["Semester"];
                    response.IdStudy = (int)dr["IdStudy"];
                    response.StartDate = DateTime.ParseExact(dr["StartDate"].ToString(), "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                }
            }
            return response;
        }

    }
}
