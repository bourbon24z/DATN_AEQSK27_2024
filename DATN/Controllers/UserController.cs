using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly StrokeDbContext _context;

        public UserController(StrokeDbContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.StrokeUsers.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username already exists.");
            }

            // hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // new Stroke user
            var user = new StrokeUser
            {
                Username = registerDto.Username,
                Password = hashedPassword,
                Role = registerDto.Role
            };

            
            if (registerDto.Patient != null)
            {
                var patient = new Patient
                {
                    PatientName = registerDto.Patient.PatientName,
                    DateOfBirth = registerDto.Patient.DateOfBirth,
                    Gender = registerDto.Patient.Gender,
                    Phone = registerDto.Patient.Phone,
                    Email = registerDto.Patient.Email,
                    CreatedAt = DateTime.UtcNow  
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                user.UserPatientId = patient.PatientId;
            }

            _context.StrokeUsers.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registration successful.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var dbUser = await _context.StrokeUsers
                .SingleOrDefaultAsync(u => u.Username == loginDto.Username);

            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok("Login successful.");
        }
    }
}
