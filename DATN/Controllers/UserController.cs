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
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            
            var existingUser = await _context.StrokeUsers
                .Where(u => u.Email == registerUserDto.Email || u.Phone == registerUserDto.Phone || u.Username == registerUserDto.Username)
                .Select(u => new
                {
                    EmailExists = u.Email == registerUserDto.Email,
                    PhoneExists = u.Phone == registerUserDto.Phone,
                    UsernameExists = u.Username == registerUserDto.Username
                })
                .FirstOrDefaultAsync();

            
            if (existingUser != null)
            {
                var errors = new List<string>();
                if (existingUser.EmailExists) errors.Add("Email đã tồn tại.");
                if (existingUser.PhoneExists) errors.Add("Số điện thoại đã tồn tại.");
                if (existingUser.UsernameExists) errors.Add("Username đã tồn tại.");

                return BadRequest(string.Join(" ", errors)); 
            }

            // Tạo người dùng tạm thời nếu không có lỗi
            var tempUser = new UserRegistrationTemp
            {
                Username = registerUserDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
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

            
            await _emailService.SendEmailAsync(registerUserDto.Email, "OTP Confirmation", $"Your OTP is: {tempUser.Otp}");

            return Ok("OTP đã được gửi đến email của bạn. Vui lòng xác nhận.");
        }


        [HttpPost("verifyotp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var tempUser = await _context.UserRegistrationTemps
                .SingleOrDefaultAsync(u => u.Email == verifyOtpDto.Email && u.Otp == verifyOtpDto.Otp);

            if (tempUser == null || tempUser.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest("OTP không hợp lệ hoặc đã hết hạn.");
            }

            var newUser = new StrokeUser
            {
                Username = tempUser.Username,
                Password = tempUser.Password,
                PatientName = tempUser.PatientName,
                DateOfBirth = tempUser.DateOfBirth,
                Gender = tempUser.Gender,
                Phone = tempUser.Phone,
                Email = tempUser.Email,
                CreatedAt = DateTime.UtcNow,
                IsVerified = true
            };
            _context.StrokeUsers.Add(newUser);
            await _context.SaveChangesAsync();

            
            var userRole = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == "user");
            if (userRole != null)
            {
                var newUserRole = new UserRole
                {
                    UserId = newUser.UserId,
                    RoleId = userRole.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserRoles.Add(newUserRole);
                await _context.SaveChangesAsync();
            }

            _context.UserRegistrationTemps.Remove(tempUser);
            await _context.SaveChangesAsync();

            return Ok("Email đã được xác minh và đăng ký thành công.");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var dbUser = await _context.StrokeUsers
                .SingleOrDefaultAsync(u =>
                    u.Username == loginDto.Credential ||
                    u.Email == loginDto.Credential ||
                    u.Phone == loginDto.Credential);

            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, dbUser.Password))
            {
                return Unauthorized("Username hoặc mật khẩu không đúng.");
            }

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == dbUser.UserId && ur.IsActive)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("huynguyencutephomaiquenhatthegioi12345!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString()),
            new Claim(ClaimTypes.Role, string.Join(",", roles))
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "localhost:5062",
                Audience = "localhost:5062",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                message = "Đăng nhập thành công.",
                data = new
                {
                    dbUser.UserId,
                    dbUser.Username,
                    dbUser.DateOfBirth,
                    dbUser.Email,
                    dbUser.Gender,
                    dbUser.Phone,
                    Roles = roles,
                    Token = tokenHandler.WriteToken(token)
                }
            });
        }
        //http://localhost:5062/api/User/update-basic-info
        [HttpPut("update-basic-info")]
        [Authorize]
        public async Task<IActionResult> UpdateBasicInfo([FromBody] UpdateBasicInfoDto updateBasicInfoDto)
        {
            // get user data JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var dbUser = await _context.StrokeUsers.FindAsync(userId);

            if (dbUser == null)
            {
                return NotFound("User not found.");
            }

            // put info
            dbUser.PatientName = updateBasicInfoDto.PatientName ?? dbUser.PatientName;
            dbUser.DateOfBirth = updateBasicInfoDto.DateOfBirth != default ? updateBasicInfoDto.DateOfBirth : dbUser.DateOfBirth;
            dbUser.Gender = updateBasicInfoDto.Gender;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Basic information updated successfully.",
                data = new
                {
                    dbUser.PatientName,
                    dbUser.DateOfBirth,
                    dbUser.Gender
                }
            });
        }
        [HttpGet("users/{id}")]
// http://localhost:5062/api/admin/users/123
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.StrokeUsers.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var roles = await _context.UserRoles.AsNoTracking()
                    .Where(ur => ur.UserId == user.UserId && ur.IsActive)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync();

                var userDto = new
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Roles = roles,
                    PatientName = user.PatientName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
