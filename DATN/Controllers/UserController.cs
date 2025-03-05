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
        public async Task<IActionResult> Register( RegisterDto registerDto)
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

		[HttpPut("patient/{userId}")]
		public async Task<IActionResult> UpdatePatient([FromRoute] int userId, [FromBody] PatientDto patientDto)
		{
			var dbUser = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
			if (dbUser == null)
			{
				return NotFound("User not found.");
			}
			var dbPatient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == dbUser.UserPatientId);
			if (dbPatient == null)
			{
				return NotFound("User not found.");
			}

			dbPatient.PatientName = patientDto.PatientName;
			dbPatient.DateOfBirth = patientDto.DateOfBirth;
			dbPatient.Gender = patientDto.Gender;
			dbPatient.Phone = patientDto.Phone;
			dbPatient.Email = patientDto.Email;
			await _context.SaveChangesAsync();
			return Ok(patientDto);
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

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                
                Console.WriteLine($"Received request for user with id: {id}");

                var user = await _context.StrokeUsers
                    .Include(u => u.Patient)
                    .SingleOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return NotFound("User not found.");
                }

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role,
                    Patient = user.Patient == null ? null : new Dto.PatientDto
                    {
                        PatientName = user.Patient.PatientName,
                        DateOfBirth = user.Patient.DateOfBirth,
                        Gender = user.Patient.Gender,
                        Phone = user.Patient.Phone,
                        Email = user.Patient.Email
                    }
                };

                Console.WriteLine("User found and returned successfully.");
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.StrokeUsers
                    .Include(u => u.Patient)
                    .SingleOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                _context.StrokeUsers.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
