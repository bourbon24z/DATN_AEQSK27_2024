using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using DATN.Verification;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly EmailService _emailService;

        public UserController(StrokeDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register( RegisterDto registerDto)
        {
            if (await _context.StrokeUsers.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username already exists.");
            }
            //otp
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(15);

            // save cache infor dto
            var tempUser = new UserRegistrationTemp
            {
                Username = registerDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                Email = registerDto.Patient.Email,
                Otp = otp,
                OtpExpiry = otpExpiry
            };

            _context.UserRegistrationTemps.Add(tempUser);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(registerDto.Patient.Email, "OTP Confirmation", $"Your OTP is: {otp}");

            return Ok("OTP has been sent to your email. Please verify.");
        }
        [HttpPost("verifyotp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var tempUser = await _context.UserRegistrationTemps
                .SingleOrDefaultAsync(u => u.Email == verifyOtpDto.Email && u.Otp == verifyOtpDto.Otp);

            if (tempUser == null || tempUser.OtpExpiry < DateTime.UtcNow) 
            {
                return BadRequest("Invalid or expired OTP.");
            }

            // check infor patient
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == tempUser.Email);
            if (patient == null)
            {
                patient = new Patient
                {
                    PatientName = verifyOtpDto.PatientName,
                    DateOfBirth = verifyOtpDto.DateOfBirth,
                    Gender = verifyOtpDto.Gender,
                    Phone = verifyOtpDto.Phone,
                    Email = tempUser.Email,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            //add patient before user
            patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == tempUser.Email);
            if (patient == null)
            {
                return StatusCode(500, "Failed to create patient record.");
            }

            // new stroke user
            var user = new StrokeUser
            {
                Username = tempUser.Username,
                Password = tempUser.Password,
                Role = tempUser.Role,
                UserPatientId = patient.PatientId
            };

            _context.StrokeUsers.Add(user);
            await _context.SaveChangesAsync();

            // remove cached temp user
            _context.UserRegistrationTemps.Remove(tempUser);
            await _context.SaveChangesAsync();

            return Ok("Email verified and registration successful.");
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
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            var verification = await _context.UserVerifications.SingleOrDefaultAsync(v => v.VerificationCode == code);
            if (verification == null)
            {
                return NotFound("Invalid verification code.");
            }

            verification.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok("Email confirmed successfully.");
        }
    }

}

