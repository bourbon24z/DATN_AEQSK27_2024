using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using DATN.Verification;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        //http://localhost:5062/api/User/register
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            var existingUser = await _context.StrokeUsers.SingleOrDefaultAsync(u =>
                      u.Email ==  registerUserDto.Email ||
                      u.Phone ==  registerUserDto.Phone ||
                      u.Username == registerUserDto.Username);
            await _context.UserRegistrationTemps.AnyAsync(u =>
                      u.Email == registerUserDto.Email || 
                      u.Phone == registerUserDto.Phone ||
                      u.Username == registerUserDto.Username);
            if (existingUser != null)
            {
                return BadRequest("Mail hoặc sdt hoặc username tồn tại rồi fennn, m tính mạo danh à!!!! Kao báo ông can còng đầu chết cụ m nháaaaa");
            }

            var tempUser = new UserRegistrationTemp
            {
                Username = registerUserDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
                Role = registerUserDto.Role,
                Email = registerUserDto.Email,
                Otp = new Random().Next(100000, 999999).ToString(),
                OtpExpiry = DateTime.UtcNow.AddMinutes(15),
                PatientName = registerUserDto.PatientName,
                DateOfBirth = registerUserDto.DateOfBirth,
                Gender = registerUserDto.Gender,
                Phone = registerUserDto.Phone
            };

            _context.UserRegistrationTemps.Add(tempUser);
            await _context.SaveChangesAsync();

            // send otp to email
            await _emailService.SendEmailAsync(registerUserDto.Email, "OTP Confirmation", $"Your OTP is: {tempUser.Otp}");

            return Ok("OTP has been sent to your email. Please verify.");
        }

        [HttpPost("verifyotp")]
        //http://localhost:5062/api/user/verifyotp
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var tempUser = await _context.UserRegistrationTemps
                .SingleOrDefaultAsync(u => u.Email == verifyOtpDto.Email && u.Otp == verifyOtpDto.Otp);

            if (tempUser == null || tempUser.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            var user = new StrokeUser
            {
                Username = tempUser.Username,
                Password = tempUser.Password,
                Role = tempUser.Role,
                PatientName = tempUser.PatientName,
                DateOfBirth = tempUser.DateOfBirth,
                Gender = tempUser.Gender,
                Phone = tempUser.Phone,
                Email = tempUser.Email,
                CreatedAt = DateTime.UtcNow,
                IsVerified = true
            };

            _context.StrokeUsers.Add(user);
            await _context.SaveChangesAsync();

            // remove temp user
            _context.UserRegistrationTemps.Remove(tempUser);
            await _context.SaveChangesAsync();

            return Ok("Email verified and registration successful.");
        }


        [HttpPost("login")]
        //http://localhost:5062/api/User/login
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var dbUser = await _context.StrokeUsers
                .SingleOrDefaultAsync(u =>
                u.Username == loginDto.Credential ||
                u.Email == loginDto.Credential ||
                u.Phone == loginDto.Credential);

            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.Password))
            {
                return Unauthorized("Invalid username or password.");
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("huynguyencutephomaiquenhatthegioi12345!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "localhost:5062",
                Audience = "localhost:5062",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var userDto = new UserDto
            {
                UserId = dbUser.UserId,
                Username = dbUser.Username,
                Role = dbUser.Role,
                PatientName = dbUser.PatientName,
                DateOfBirth = dbUser.DateOfBirth,
                Gender = dbUser.Gender,
                Phone = dbUser.Phone,
                Email = dbUser.Email,
                Token = tokenString
            };
            return Ok(new
            {
                message = "Login successfully.",
                //token = tokenHandler.WriteToken(token),
                data = userDto
            });
            //return Ok(new
            //{
            //    message = "Login successful.",
            //    data = userDto
            //}); 
        }

        [HttpGet("user/{id}")]
        //http://localhost:5062/api/User/user/{id}
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                Console.WriteLine($"Received request for user with id: {id}");

                var user = await _context.StrokeUsers
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
                    PatientName = user.PatientName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
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

        [HttpGet("users")]
        //http://localhost:5062/api/User/users
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                
                var users = await _context.StrokeUsers.ToListAsync();

                
                var userDtos = users.Select(user => new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role,
                    PatientName = user.PatientName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                }).ToList();

                return Ok(userDtos); 
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return StatusCode(500, "Internal server error occurred.");
            }
        }

        [HttpDelete("user/{id}")]
        //http://localhost:5062/api/User/user/{id}
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.StrokeUsers
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
