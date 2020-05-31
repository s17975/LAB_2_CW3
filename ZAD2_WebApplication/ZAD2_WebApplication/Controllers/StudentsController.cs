
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ZAD2_WebApplication.DAL;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        public IConfiguration _configuration { get; set; }

        // INICJUJ DBService i IConfiguration
        public StudentsController(IStudentsDbService dbService, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbService = dbService;
        }

        // POBIERZ WSZYTKICH STUDENTÓW z dbo.Students
        [Authorize(Roles = "student")]
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }

        // POBIERZ STUDENTA PO IndexNumber z dbo.Students
        [Authorize(Roles = "student")]
        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            return Ok(_dbService.GetStudent(index));
        }

        //  USUŃ STUDENTA PO IndexNumber z dbo.Students
        [Authorize(Roles = "employee")]
        [HttpDelete("{index}")]
        public IActionResult deleteStudent(string index)
        {
            int rowsAff = _dbService.DeleteStudent(index);
            return Ok("Usuwanie ukończone, usunięto : "+rowsAff.ToString()+" wierszy");
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
            var claims = new Claim [studentAutorizationRoles.DB_Roles.Count()];
            foreach(string role in studentAutorizationRoles.DB_Roles) 
            { 
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
            _dbService.SetRefreshToken(refreshToken,studentAuthentication.IndexNumber);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        // Odświeżenie/zapisanie RefreshToken
        [AllowAnonymous]
        [HttpPost("refreshToken/{token}")]
        public IActionResult RefreshToken(string refreshToken) {
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






        /// --------------------------------------------------------------- ///
        ///       METODY Z ZAŚLEPKĄ BAZY DANYCH - Ćwiczenia numer 3         ///
        /// --------------------------------------------------------------- ///

        //  POBIERZ STUDENTA z zaślepki bazy
        /*
        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents());
        }
        */
        //  POBIERZ STUDENTA + orderBy
        /*
        [HttpGet]
        public string GetStudent(string orderBy)
        {
            return $"Rudko, Rudko2, Arudko sortowanie={orderBy}";
        }
        */
        //  POBIERZ STUDENTA po ID
        /*
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Rudko");
            }
            else if (id == 2)
            {
                return Ok("Inny Rudko");
            }
            return NotFound("Nie znalezniono studenta");
        }
        */
        //  DODAJ STUDENTA random
        /*
        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }
        */
        //  AKTUALIZUJ STUDENTA po ID
        /*
        [HttpPut("{id}")]
        public IActionResult putStudent(int id)
        {
            return Ok("Aktualizacja dokończona");
        }
        */
        //  USUŃ STUDENTA po ID
        /*
        [HttpDelete("{id}")]
        public IActionResult deleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }
        */
    }
}