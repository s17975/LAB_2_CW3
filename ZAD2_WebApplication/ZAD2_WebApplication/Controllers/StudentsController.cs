using System;
using System.Collections.Generic;
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
        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }
        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents());
        }
        [HttpGet]
        public string GetStudent(string orderBy)
        {
            return $"Rudko, Rudko2, Arudko sortowanie={orderBy}";
        }
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Rudko");
            } else if (id == 2)
            {
                return Ok("Inny Rudko");
            }
            return NotFound("Nie znalezniono studenta");
        }
        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }
        [HttpPut("{id}")]
        public IActionResult putStudent(int id)
        {
            return Ok("Aktualizacja dokończona");
        }
        [HttpDelete("{id}")]
        public IActionResult deleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }
    }
}