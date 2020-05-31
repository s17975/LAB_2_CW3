using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ZAD2_WebApplication.DAL;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.Controllers
{
    [Authorize]
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        public IConfiguration _configuration { get; set; }

        // INICJUJ DBService
        public EnrollmentsController(IStudentsDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            _configuration = configuration;
        }

        // DODAJ STUDENTA
        [Route("api/enrollments")]
        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(string IndexNumber, string FirstName, string LastName, string BirthDate, string Studies)
        {
            
            if(string.IsNullOrEmpty(IndexNumber) || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(BirthDate) || string.IsNullOrEmpty(Studies))
            {
                Console.WriteLine("Parametry żądania mają niepoprawną wartość");
                return StatusCode(400);
            } else
            {
                try
                {
                    Request_EnrollStudent request = new Request_EnrollStudent();
                    request.IndexNumber = IndexNumber;
                    request.FirstName = FirstName;
                    request.LastName = LastName;
                    request.BirthDate = DateTime.ParseExact(BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    request.Studies = Studies;

                    Response_Enrollment response = _dbService.EnrollStudent(request);

                    return Ok(response);
                } catch (Exception ex)
                {
                    Console.WriteLine("Blad przy wywolaniu db.EnrollStudent : "+ex.Message.ToString());
                    return StatusCode(400);
                }
            } 
        }

        // STUDENT LEVEL UP
        [Route("promotions")]
        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(int Semester, string Studies)
        {
            Console.WriteLine(User.Claims.ToList().Count());
            if (Semester<=0 || Semester > 10 || string.IsNullOrEmpty(Studies))
            {
                Console.WriteLine("Parametry żądania mają niepoprawną wartość");
                return StatusCode(400);
            } else
            {
                try
                {
                    Response_Enrollment response = _dbService.PromoteStudents(Semester, Studies);
                    return Ok(response);
                } catch (Exception ex)
                {
                    Console.WriteLine("Blad przy wywolaniu db.PromoteStudents : " + ex.Message.ToString());
                    return StatusCode(404);
                }
                
            }
        }

        // Autoryzacja Studenta po IndexNumber && Password
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Login(Request_StudentAuthentication request)
        {
            // AUTENTYKACJA STUDENTA
            Response_StudentAuthentication studentAuthentication = _dbService.AuthenticateStudent(request);
            // WERYFIKACJA HASH HASLA
            string hashed_password_from_request = HashPassword(request.Password, studentAuthentication.Salt);
            if (hashed_password_from_request != studentAuthentication.Password)
            {
                return StatusCode(401);
            }

            // USTALENIE PRZYPISANYCH RÓL
            Response_AuthorizationRoles studentAutorizationRoles = _dbService.GetAuthorization(studentAuthentication.IndexNumber);
            var claims = new Claim[studentAutorizationRoles.DB_Roles.Count()];

            foreach (string role in studentAutorizationRoles.DB_Roles)
            {
                Console.WriteLine(role);
                new Claim(ClaimTypes.Name, request.IndexNumber);
                new Claim(ClaimTypes.Role, role);
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "GakkoDB",
                audience: "All",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid();

            // ADD/UPDATE RefreshToken in DB
            _dbService.SetRefreshToken(refreshToken, studentAuthentication.IndexNumber);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        // Odświeżenie/zapisanie RefreshToken
        [AllowAnonymous]
        [HttpPost("refreshToken/{token}")]
        public IActionResult RefreshToken(string refreshToken)
        {
            string IndexNumber = _dbService.GetRefreshTokenOwner(refreshToken);
            Response_AuthorizationRoles studentAutorizationRoles = _dbService.GetAuthorization(IndexNumber);
            var claims = new Claim[studentAutorizationRoles.DB_Roles.Count()];
            foreach (string role in studentAutorizationRoles.DB_Roles)
            {
                new Claim(ClaimTypes.Name, IndexNumber);
                new Claim(ClaimTypes.Role, role);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "GakkoDB",
                audience: "All",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var newRefreshToken = Guid.NewGuid();

            // ADD/UPDATE RefreshToken in DB
            _dbService.SetRefreshToken(newRefreshToken, IndexNumber);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                newRefreshToken
            });
        }

        // Haszuj haslo
        public static string HashPassword(string pass, string salt)
        {
            string HashedPassword = Convert.ToBase64String(
                    KeyDerivation.Pbkdf2(
                        password: pass,
                        salt: Encoding.UTF8.GetBytes(salt),
                        prf: KeyDerivationPrf.HMACSHA512,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8
                    ));
            return HashedPassword;
        }
    }
}