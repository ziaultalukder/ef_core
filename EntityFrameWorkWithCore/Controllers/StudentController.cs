using EntityFrameWorkWithCore.Context;
using EntityFrameWorkWithCore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EntityFrameWorkWithCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbConnection _connection;
        private readonly IConfiguration _configuration;

        public StudentController(ApplicationDbContext context, IDbConnection connection, IConfiguration configuration)
        {
            _context = context;
            _connection = connection;
            _configuration = configuration;
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> AddStudent(Student student)
        {
            _context.Students.Add(student);
            if (_context.SaveChanges() > 0)
            {
                return Ok("Save Success");
            }
            else
            {
                return Ok("Save Failed");
            }
        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> GetStudent()
        {
            string dst = _configuration.GetConnectionString("DefaultConnection");
            //var std = _context.Students.ToList();
            return Ok(dst);
        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> GetStudent1()
        {
            throw new NotImplementedException("this method is not implement");
        }
    }
}
