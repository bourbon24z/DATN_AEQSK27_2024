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
using DATN.Configuration;

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
            // check  existing user
            var existingUser = await _context.StrokeUsers
                .Where(u => 
                u.Email == registerUserDto.Email || 
                u.Phone == registerUserDto.Phone || 
                u.Username == registerUserDto.Username)
                .Select(u => new { u.Email,
                                   u.Phone,
                                   u.Username})
                .FirstOrDefaultAsync();
            var existingTempUser = await _context.UserRegistrationTemps
                .Where(u => 
                u.Email == registerUserDto.Email ||
                u.Phone == registerUserDto.Phone ||
                u.Username == registerUserDto.Username)
                .Select(u => new { u.Email, 
                                   u.Phone, 
                                   u.Username })
                .FirstOrDefaultAsync();
            if (existingUser != null)
            {
                var errors = new List<string>();
                if (existingUser.Email == registerUserDto.Email)
                    errors.Add("The email already exists.");
                if (existingUser.Phone == registerUserDto.Phone)
                    errors.Add("The phone number already exists.");
                if (existingUser.Username == registerUserDto.Username)
                    errors.Add("The username already exists.");
                return BadRequest(string.Join(" ", errors));
            }

            var otpCode = new Random().Next(100000, 999999).ToString();  
            var tempUser = new UserRegistrationTemp
            {
                Username = registerUserDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
                Email = registerUserDto.Email,
                Otp = otpCode,
                OtpExpiry = DateTime.Now.AddMinutes(15),
                PatientName = registerUserDto.PatientName,
                DateOfBirth = registerUserDto.DateOfBirth,
                Gender = registerUserDto.Gender,
                Phone = registerUserDto.Phone
            };

            _context.UserRegistrationTemps.Add(tempUser);
            await _context.SaveChangesAsync();

            
            var emailQueue = HttpContext.RequestServices.GetRequiredService<IBackgroundEmailQueue>();
            emailQueue.EnqueueEmail(async () =>
            {
                await _emailService.SendEmailAsync(
                    registerUserDto.Email,
                    "OTP Confirmation",
                    $"Your OTP is: {otpCode}");
            });

            return Ok("OTP has been sent.");
        }



        [HttpPost("verifyotp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var tempUser = await _context.UserRegistrationTemps
                .SingleOrDefaultAsync(u => u.Email == verifyOtpDto.Email && u.Otp == verifyOtpDto.Otp);

            if (tempUser == null || tempUser.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest("OTP invalid, please try again.");
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
                CreatedAt = DateTime.Now,
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

            return Ok("The email has been successfully verified and registered..");
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
                return Unauthorized("Incorrect username or password.");
            }

         
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == dbUser.UserId && ur.IsActive)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("huynguyencutephomaiquenhatthegioi12345!");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString())
    };

            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "localhost:5062",
                Audience = "localhost:5062",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                message = "Login Succesfully.",
                data = new
                {
                    dbUser.UserId,
                    dbUser.Username,
                    dbUser.PatientName,
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

        
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        //http://localhost:5062/api/User/forgot-password
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            
            var user = await _context.StrokeUsers.SingleOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
            if (user == null)
            {
               
                return Ok("If this email exists in our system, an OTP has been sent for password reset.");
            }

            
            var otp = new Random().Next(100000, 999999).ToString();

            
            var userVerification = new UserVerification
            {
                UserId = user.UserId,
                Email = user.Email,
                VerificationCode = otp,
                OtpExpiry = DateTime.UtcNow.AddMinutes(15), 
                IsVerified = false
            };
            _context.UserVerifications.Add(userVerification);
            await _context.SaveChangesAsync();

            
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Reset OTP",
                $"Your OTP for password reset is: {otp}. It will expire in 15 minutes."
            );

            return Ok("OTP has been sent to your email for password reset.");
        }
        [HttpPost("reset-password")]
        [AllowAnonymous]
        //http://localhost:5062/api/User/reset-password
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            
            var user = await _context.StrokeUsers.SingleOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            if (user == null)
            {
                return BadRequest("Invalid request.");
            }

            
            var verification = await _context.UserVerifications
                .Where(v => v.Email == resetPasswordDto.Email &&
                            v.VerificationCode == resetPasswordDto.Otp &&
                            !v.IsVerified)
                .OrderByDescending(v => v.OtpExpiry)
                .FirstOrDefaultAsync();

            if (verification == null || verification.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            
            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            // mark otp has been used
            verification.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }
        [HttpPut("change-password")]
        [Authorize]
        //http://localhost:5062/api/User/change-password
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            // get by id
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid user identifier.");
            }

            // find by id
            var user = await _context.StrokeUsers.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // compare old and new pass
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
            {
                return BadRequest("Current password is incorrect.");
            }

          
            // check new pass dont match current pass
            if (changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
            {
                return BadRequest("The new password must be different from the current password.");
            }

            
            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been updated successfully." });
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

        [HttpPost("user-gps")]
        
        public async Task<IActionResult> PostUserGPS([FromBody] UserGpsDto userGpsDto)
        {
            var user = await _context.StrokeUsers
                .FirstOrDefaultAsync(u => u.UserId == userGpsDto.UserId);
            var existingGps = await _context.Gps
                .FirstOrDefaultAsync(g => g.UserId == user.UserId);
            if (existingGps != null)
            {
                _context.Gps.Remove(existingGps);
            }
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var gpsData = new Gps
            {
                UserId = user.UserId,
                Lon = userGpsDto.Long,
                Lat = userGpsDto.Lat,
                CreatedAt = DateTime.UtcNow
            };
            _context.Gps.Add(gpsData);
            await _context.SaveChangesAsync();
            return Ok("GPS data saved successfully.");
        }

        [HttpGet("user-gps")]
        
        public async Task<IActionResult> GetUserGPS(int userId)
        {
            var user = await _context.StrokeUsers
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var gpsData = await _context.Gps.FirstOrDefaultAsync(g => g.UserId == userId);
			if (gpsData == null)
			{
				return NotFound("GPS data not found.");
			}
			return Ok(gpsData);
        }


    }


}

