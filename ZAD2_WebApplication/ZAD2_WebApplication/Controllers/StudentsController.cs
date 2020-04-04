using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZAD2_WebApplication.DAL;
using ZAD2_WebApplication.Models;

namespace ZAD2_WebApplication.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        // INICJUJ BAZE DANYCH
        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        // POBIERZ WSZYTKICH STUDENTÓW z dbo.Students
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }

        // POBIERZ STUDENTA PO indexNumber z dbo.Students
        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            return Ok(_dbService.GetStudent(index));
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
        //  DODAJ STUDENTA po ID
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