using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZAD2_WebApplication.DAL;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.Controllers
{
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;

        // INICJUJ DBService
        public EnrollmentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }

        // DODAJ STUDENTA
        [Route("api/enrollments")]
        [HttpPost]
        public IActionResult EnrollStudent(string IndexNumber, string FirstName, string LastName, string BirthDate, string Studies)
        {
            
            if(string.IsNullOrEmpty(IndexNumber) || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(BirthDate) || string.IsNullOrEmpty(Studies))
            {
                Console.WriteLine("cos jest nullem");
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
        [Route("api/enrollments/promotions")]
        [HttpPost]
        public IActionResult PromoteStudents(int semester, string studies)
        {
            _dbService.PromoteStudents(semester, studies);
            var response = semester.ToString()+studies;

            return Ok(response);
        }

    }
}