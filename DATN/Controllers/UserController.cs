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
                .Select(u => new
                {
                    u.Email,
                    u.Phone,
                    u.Username
                })
                .FirstOrDefaultAsync();
            var existingTempUser = await _context.UserRegistrationTemps
                .Where(u =>
                u.Email == registerUserDto.Email ||
                u.Phone == registerUserDto.Phone ||
                u.Username == registerUserDto.Username)
                .Select(u => new
                {
                    u.Email,
                    u.Phone,
                    u.Username
                })
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
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }

            
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == dbUser.UserId && ur.IsActive)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.RoleId,
                    (ur, r) => r.RoleName)
                .Distinct() 
                .ToListAsync();

            
            if (roles.Count == 0)
            {
                return Unauthorized("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
            }

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
                message = "Đăng nhập thành công.",
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

                return Unauthorized("Email don't exist");
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


        //[HttpGet("users/{id}")]
        //// http://localhost:5062/api/admin/users/123
        //public async Task<IActionResult> GetUserById(int id)
        //{
        //    try
        //    {
        //        var user = await _context.StrokeUsers.AsNoTracking()
        //            .FirstOrDefaultAsync(u => u.UserId == id);

        //        if (user == null)
        //        {
        //            return NotFound("User not found.");
        //        }

        //        var roles = await _context.UserRoles.AsNoTracking()
        //            .Where(ur => ur.UserId == user.UserId && ur.IsActive)
        //            .Select(ur => ur.Role.RoleName)
        //            .ToListAsync();

        //        var userDto = new
        //        {
        //            UserId = user.UserId,
        //            Username = user.Username,
        //            Roles = roles,
        //            PatientName = user.PatientName,
        //            DateOfBirth = user.DateOfBirth,
        //            Gender = user.Gender,
        //            Phone = user.Phone,
        //            Email = user.Email
        //        };

        //        return Ok(userDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}
        [HttpGet("users/{id}")]
        [Authorize]
        // http://localhost:5062/api/user/users/123
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {

                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(currentUserIdStr, out int currentUserId))
                {
                    return BadRequest("Invalid user identifier");
                }


                var currentUserRoles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();


                bool hasAccess = false;

                if (currentUserId == id)
                {

                    hasAccess = true;
                }
                else if (currentUserRoles.Contains("admin"))
                {

                    hasAccess = true;
                }
                else if (currentUserRoles.Contains("doctor"))
                {

                    var userRoles = await _context.UserRoles
                        .Where(ur => ur.UserId == id && ur.IsActive)
                        .Select(ur => ur.Role.RoleName)
                        .ToListAsync();

                    hasAccess = userRoles.Contains("user");
                }

                if (!hasAccess)
                {
                    return Forbid("You don't have permission to view this user's information");
                }

                var user = await _context.StrokeUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                var roles = await _context.UserRoles
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



        [HttpGet("me")]
        [Authorize]
        //http://localhost:5062/api/user/me
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {

                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }


                var user = await _context.StrokeUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found");


                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync();

                return Ok(new
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Roles = roles,
                    PatientName = user.PatientName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("my-doctors")]
        [Authorize]
        //http://localhost:5062/api/user/my-doctors
        public async Task<IActionResult> GetMyDoctors()
        {
            try
            {

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }


                var relationships = await _context.Relationships
                    .Include(r => r.Inviter)
                    .Where(r => r.UserId == userId && r.RelationshipType == "doctor-patient")
                    .ToListAsync();

                if (!relationships.Any())
                {
                    return Ok(new
                    {
                        Message = "You don't have any doctors assigned.",
                        Doctors = new List<object>()
                    });
                }


                var doctors = relationships.Select(r => new
                {
                    DoctorId = r.Inviter.UserId,
                    DoctorName = r.Inviter.PatientName,
                    Email = r.Inviter.Email,
                    Phone = r.Inviter.Phone,
                    RelationshipId = r.RelationshipId,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return Ok(new
                {
                    TotalCount = doctors.Count,
                    Doctors = doctors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("use-invitation-code")]
        [Authorize]
        //http://localhost:5062/api/user/use-invitation-code
        public async Task<IActionResult> UseInvitationCode([FromBody] UseInvitationCodeDto model)
        {
            try
            {

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }
                var userRoles = User.Claims
           .Where(c => c.Type == ClaimTypes.Role)
           .Select(c => c.Value)
           .ToList();

                if (userRoles.Contains("doctor") || userRoles.Contains("admin"))
                {
                    return BadRequest("Doctors and administrators cannot be patients. Please use a patient account.");
                }

                var invitationCode = await _context.InvitationCodes
                    .Include(ic => ic.InviterUser)
                    .FirstOrDefaultAsync(ic =>
                        ic.Code == model.Code &&
                        ic.Status == "active" &&
                        ic.ExpiresAt > DateTime.UtcNow);

                if (invitationCode == null)
                {
                    return BadRequest("Invalid or expired invitation code");
                }


                var isDoctorInviter = await _context.UserRoles
                    .AnyAsync(ur =>
                        ur.UserId == invitationCode.InviterUserId &&
                        ur.Role.RoleName == "doctor" &&
                        ur.IsActive);

                if (!isDoctorInviter)
                {
                    return BadRequest("This invitation code was not created by a doctor");
                }


                var existingRelationship = await _context.Relationships
                    .AnyAsync(r =>
                        r.UserId == userId &&
                        r.InviterId == invitationCode.InviterUserId &&
                        r.RelationshipType == "doctor-patient");

                if (existingRelationship)
                {
                    return BadRequest("You are already connected to this doctor");
                }


                var relationship = new Relationship
                {
                    UserId = userId,
                    InviterId = invitationCode.InviterUserId,
                    RelationshipType = "doctor-patient",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Relationships.Add(relationship);


                invitationCode.Status = "used";

                await _context.SaveChangesAsync();


                var doctor = invitationCode.InviterUser;

                return Ok(new
                {
                    Message = "Successfully connected to doctor",
                    DoctorId = doctor.UserId,
                    DoctorName = doctor.PatientName,
                    Email = doctor.Email,
                    Phone = doctor.Phone
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }


        [HttpGet("health-profile")]
        [Authorize]
        // http://localhost:5062/api/User/health-profile
        public async Task<IActionResult> GetHealthProfile()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

                var user = await _context.StrokeUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found");

                
                var devices = await _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => new
                    {
                        d.DeviceId,
                        d.DeviceName,
                        d.DeviceType,
                        d.Series
                    })
                    .ToListAsync();

             
                var latestMedicalData = new Dictionary<string, object>();

                if (devices.Any())
                {
                    
                    var deviceIds = devices.Select(d => d.DeviceId).ToList();

                    
                    var medicalDataQuery = from d in _context.UserMedicalDatas
                                           join dev in _context.Device on d.DeviceId equals dev.DeviceId
                                           where deviceIds.Contains(dev.DeviceId)
                                           group d by dev.DeviceType into g
                                           select g.OrderByDescending(x => x.RecordedAt).FirstOrDefault();

                    var latestData = await medicalDataQuery.ToListAsync();

                    foreach (var data in latestData)
                    {
                        if (data != null)
                        {
                            var device = await _context.Device.FindAsync(data.DeviceId);
                            string deviceName = device?.DeviceName ?? "Unknown Device";

                            latestMedicalData[device?.DeviceType ?? "Unknown"] = new
                            {
                                DeviceName = deviceName,
                                RecordedAt = data.RecordedAt,
                                HeartRate = data.HeartRate,
                                SystolicPressure = data.SystolicPressure,
                                DiastolicPressure = data.DiastolicPressure,
                                Temperature = data.Temperature,
                                SpO2 = data.Spo2Information,
                                BloodPh = data.BloodPh
                            };
                        }
                    }
                }

                
                var clinicalIndicator = await _context.ClinicalIndicators
                    .Where(ci => ci.UserID == userId && ci.IsActived)
                    .OrderByDescending(ci => ci.RecordedAt)
                    .FirstOrDefaultAsync();

                var molecularIndicator = await _context.MolecularIndicators
                    .Where(mi => mi.UserID == userId && mi.IsActived)
                    .OrderByDescending(mi => mi.RecordedAt)
                    .FirstOrDefaultAsync();

                var subclinicalIndicator = await _context.SubclinicalIndicators
                    .Where(si => si.UserID == userId && si.IsActived)
                    .OrderByDescending(si => si.RecordedAt)
                    .FirstOrDefaultAsync();

                
                var recentCaseHistories = await _context.CaseHistories
                    .Where(ch => ch.UserId == userId)
                    .OrderByDescending(ch => ch.Time)
                    .Take(5)
                    .Select(ch => new
                    {
                        CaseHistoryId = ch.CaseHistoryId,
                        Time = ch.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        StatusOfMr = ch.StatusOfMr,
                        ProgressNotes = ch.ProgressNotes != null && ch.ProgressNotes.Length > 100
                            ? ch.ProgressNotes.Substring(0, 100) + "..."
                            : ch.ProgressNotes
                    })
                    .ToListAsync();

                return Ok(new
                {
                    CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    PersonalInfo = new
                    {
                        UserId = user.UserId,
                        FullName = user.PatientName,
                        DateOfBirth = user.DateOfBirth,
                        Age = DateTime.Today.Year - user.DateOfBirth.Year,
                        Gender = user.Gender ? "Nam" : "Nữ",
                        Phone = user.Phone,
                        Email = user.Email
                    },
                    Devices = devices,
                    LatestMedicalData = latestMedicalData,
                    HealthIndicators = new
                    {
                        Clinical = clinicalIndicator,
                        Molecular = molecularIndicator,
                        Subclinical = subclinicalIndicator
                    },
                    RecentCaseHistories = recentCaseHistories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("health-records")]
        [Authorize]
        // http://localhost:5062/api/User/health-records
        public async Task<IActionResult> GetHealthRecords([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string metric = "all")
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

                
                if (!startDate.HasValue)
                    startDate = DateTime.UtcNow.AddMonths(-1);
                if (!endDate.HasValue)
                    endDate = DateTime.UtcNow;

                
                var deviceIds = await _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => d.DeviceId)
                    .ToListAsync();

                if (!deviceIds.Any())
                {
                    return Ok(new
                    {
                        Message = "No devices found for this user",
                        Records = new List<object>()
                    });
                }

              
                var query = from d in _context.UserMedicalDatas
                            join dev in _context.Device on d.DeviceId equals dev.DeviceId
                            where deviceIds.Contains(dev.DeviceId) &&
                                  d.RecordedAt >= startDate &&
                                  d.RecordedAt <= endDate
                            select new
                            {
                                DeviceId = dev.DeviceId,
                                DeviceName = dev.DeviceName,
                                DeviceType = dev.DeviceType,
                                d.RecordedAt,
                                d.HeartRate,
                                d.SystolicPressure,
                                d.DiastolicPressure,
                                d.Temperature,
                                SpO2 = d.Spo2Information,
                                d.BloodPh
                            };

               
                if (metric.ToLower() != "all")
                {
                    switch (metric.ToLower())
                    {
                        case "heartrate":
                            query = query.Where(d => d.HeartRate.HasValue);
                            break;
                        case "bloodpressure":
                            query = query.Where(d => d.SystolicPressure.HasValue || d.DiastolicPressure.HasValue);
                            break;
                        case "temperature":
                            query = query.Where(d => d.Temperature.HasValue);
                            break;
                        case "spo2":
                            query = query.Where(d => d.SpO2.HasValue);
                            break;
                        case "bloodph":
                            query = query.Where(d => d.BloodPh.HasValue);
                            break;
                        default:
                            break;
                    }
                }

                
                var records = await query
                    .OrderBy(d => d.RecordedAt)
                    .ToListAsync();

               
                var heartRateData = records.Where(r => r.HeartRate.HasValue).ToList();
                var systolicData = records.Where(r => r.SystolicPressure.HasValue).ToList();
                var diastolicData = records.Where(r => r.DiastolicPressure.HasValue).ToList();
                var temperatureData = records.Where(r => r.Temperature.HasValue).ToList();
                var spo2Data = records.Where(r => r.SpO2.HasValue).ToList();
                var bloodPhData = records.Where(r => r.BloodPh.HasValue).ToList();

                var stats = new
                {
                    TotalReadings = records.Count,
                    DateRange = new
                    {
                        From = startDate.Value.ToString("yyyy-MM-dd"),
                        To = endDate.Value.ToString("yyyy-MM-dd")
                    },
                    Metrics = new
                    {
                        HeartRate = heartRateData.Any() ? new
                        {
                            Count = heartRateData.Count,
                            Min = heartRateData.Min(r => r.HeartRate.Value),
                            Max = heartRateData.Max(r => r.HeartRate.Value),
                            Avg = heartRateData.Average(r => r.HeartRate.Value)
                        } : null,
                        BloodPressure = (systolicData.Any() || diastolicData.Any()) ? new
                        {
                            Count = systolicData.Count + diastolicData.Count,
                            SystolicAvg = systolicData.Any() ? systolicData.Average(r => r.SystolicPressure.Value) : (double?)null,
                            DiastolicAvg = diastolicData.Any() ? diastolicData.Average(r => r.DiastolicPressure.Value) : (double?)null
                        } : null,
                        Temperature = temperatureData.Any() ? new
                        {
                            Count = temperatureData.Count,
                            Min = temperatureData.Min(r => r.Temperature.Value),
                            Max = temperatureData.Max(r => r.Temperature.Value),
                            Avg = temperatureData.Average(r => r.Temperature.Value)
                        } : null,
                        SpO2 = spo2Data.Any() ? new
                        {
                            Count = spo2Data.Count,
                            Min = spo2Data.Min(r => r.SpO2.Value),
                            Max = spo2Data.Max(r => r.SpO2.Value),
                            Avg = spo2Data.Average(r => r.SpO2.Value)
                        } : null,
                        BloodPh = bloodPhData.Any() ? new
                        {
                            Count = bloodPhData.Count,
                            Min = bloodPhData.Min(r => r.BloodPh.Value),
                            Max = bloodPhData.Max(r => r.BloodPh.Value),
                            Avg = bloodPhData.Average(r => r.BloodPh.Value)
                        } : null
                    }
                };

                return Ok(new
                {
                    CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Statistics = stats,
                    Records = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("health-records/metric/{metricType}")]
        [Authorize]
        // http://localhost:5062/api/User/health-records/metric/heartrate
        public async Task<IActionResult> GetHealthRecordsByMetric(
            string metricType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

               
                if (!startDate.HasValue)
                    startDate = DateTime.UtcNow.AddMonths(-1);
                if (!endDate.HasValue)
                    endDate = DateTime.UtcNow;

               
                metricType = metricType.ToLower();
                var validMetrics = new[] { "heartrate", "bloodpressure", "temperature", "spo2", "bloodph" };
                if (!validMetrics.Contains(metricType))
                {
                    return BadRequest($"Invalid metric type. Valid options are: {string.Join(", ", validMetrics)}");
                }

                
                var deviceIds = await _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => d.DeviceId)
                    .ToListAsync();

                if (!deviceIds.Any())
                {
                    return Ok(new
                    {
                        Message = "No devices found for this user",
                        Records = new List<object>()
                    });
                }

               
                var query = from d in _context.UserMedicalDatas
                            join dev in _context.Device on d.DeviceId equals dev.DeviceId
                            where deviceIds.Contains(dev.DeviceId) &&
                                  d.RecordedAt >= startDate &&
                                  d.RecordedAt <= endDate
                            select new
                            {
                                DeviceId = dev.DeviceId,
                                DeviceName = dev.DeviceName,
                                DeviceType = dev.DeviceType,
                                d.RecordedAt,
                                d.HeartRate,
                                d.SystolicPressure,
                                d.DiastolicPressure,
                                d.Temperature,
                                SpO2 = d.Spo2Information,
                                d.BloodPh
                            };

                
                switch (metricType)
                {
                    case "heartrate":
                        query = query.Where(d => d.HeartRate.HasValue);
                        break;
                    case "bloodpressure":
                        query = query.Where(d => d.SystolicPressure.HasValue || d.DiastolicPressure.HasValue);
                        break;
                    case "temperature":
                        query = query.Where(d => d.Temperature.HasValue);
                        break;
                    case "spo2":
                        query = query.Where(d => d.SpO2.HasValue);
                        break;
                    case "bloodph":
                        query = query.Where(d => d.BloodPh.HasValue);
                        break;
                }

           
                var allData = await query.OrderBy(d => d.RecordedAt).ToListAsync();

                if (!allData.Any())
                {
                    return Ok(new
                    {
                        CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        MetricType = metricType,
                        DateRange = new { From = startDate.Value.ToString("yyyy-MM-dd"), To = endDate.Value.ToString("yyyy-MM-dd") },
                        GroupedBy = groupBy,
                        Message = "No data found for the specified metric and date range",
                        DataPoints = new List<object>()
                    });
                }

               
                List<object> groupedData;

                switch (groupBy.ToLower())
                {
                    case "hour":
                        groupedData = allData
                            .GroupBy(d => new { Date = d.RecordedAt.Date, Hour = d.RecordedAt.Hour })
                            .OrderBy(g => g.Key.Date)
                            .ThenBy(g => g.Key.Hour)
                            .Select(g => CreateMetricData(g.ToList(), metricType, $"{g.Key.Date:yyyy-MM-dd} {g.Key.Hour}:00"))
                            .ToList();
                        break;

                    case "day":
                        groupedData = allData
                            .GroupBy(d => d.RecordedAt.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => CreateMetricData(g.ToList(), metricType, g.Key.ToString("yyyy-MM-dd")))
                            .ToList();
                        break;

                    case "week":
                        groupedData = allData
                            .GroupBy(d => GetWeekNumber(d.RecordedAt))
                            .OrderBy(g => g.Key)
                            .Select(g => {
                                var items = g.ToList();
                                if (!items.Any()) return null;

                                var firstItem = items.OrderBy(d => d.RecordedAt).First();
                                var firstDayOfWeek = GetFirstDayOfWeek(firstItem.RecordedAt);
                                var lastDayOfWeek = firstDayOfWeek.AddDays(6);
                                var label = $"{firstDayOfWeek:yyyy-MM-dd} to {lastDayOfWeek:yyyy-MM-dd}";

                                return CreateMetricData(items, metricType, label);
                            })
                            .Where(g => g != null)
                            .ToList();
                        break;

                    case "month":
                        groupedData = allData
                            .GroupBy(d => new { Year = d.RecordedAt.Year, Month = d.RecordedAt.Month })
                            .OrderBy(g => g.Key.Year)
                            .ThenBy(g => g.Key.Month)
                            .Select(g => CreateMetricData(g.ToList(), metricType, $"{g.Key.Year}-{g.Key.Month:D2}"))
                            .ToList();
                        break;

                    default:
                        return BadRequest("Invalid groupBy parameter. Valid options are: hour, day, week, month");
                }

                return Ok(new
                {
                    CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    MetricType = metricType,
                    DateRange = new
                    {
                        From = startDate.Value.ToString("yyyy-MM-dd"),
                        To = endDate.Value.ToString("yyyy-MM-dd")
                    },
                    GroupedBy = groupBy,
                    DataPoints = groupedData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("devices")]
        [Authorize]
        // http://localhost:5062/api/User/devices
        public async Task<IActionResult> GetUserDevices()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

                
                var devicesQuery = from d in _context.Device
                                   where d.UserId == userId
                                   select new
                                   {
                                       d.DeviceId,
                                       d.DeviceName,
                                       d.DeviceType,
                                       d.Series,
                                       LastConnected = (from m in _context.UserMedicalDatas
                                                        where m.DeviceId == d.DeviceId
                                                        orderby m.RecordedAt descending
                                                        select m.RecordedAt).FirstOrDefault(),
                                       ReadingsCount = _context.UserMedicalDatas.Count(m => m.DeviceId == d.DeviceId)
                                   };

                var devices = await devicesQuery.ToListAsync();

                if (!devices.Any())
                {
                    return Ok(new
                    {
                        CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        Message = "No devices found for this user",
                        Devices = new List<object>()
                    });
                }

                return Ok(new
                {
                    CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalDevices = devices.Count,
                    Devices = devices
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

       
        private int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            return calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
        }

        private DateTime GetFirstDayOfWeek(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var diff = (7 + (date.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek)) % 7;
            return date.AddDays(-diff).Date;
        }

        private object CreateMetricData<T>(IEnumerable<T> groupData, string metricType, string label)
        {
            if (groupData == null || !groupData.Any())
            {
                return new { Label = label, Count = 0 };
            }

            switch (metricType)
            {
                case "heartrate":
                    {
                        var validData = groupData.Where(d => GetProperty<float?>(d, "HeartRate") != null).ToList();
                        if (!validData.Any()) return new { Label = label, Count = 0 };

                        return new
                        {
                            Label = label,
                            Count = validData.Count,
                            Min = validData.Min(d => GetProperty<float?>(d, "HeartRate").Value),
                            Max = validData.Max(d => GetProperty<float?>(d, "HeartRate").Value),
                            Average = Math.Round(validData.Average(d => GetProperty<float?>(d, "HeartRate").Value), 1),
                            Data = validData.Select(d => new
                            {
                                RecordedAt = GetProperty<DateTime>(d, "RecordedAt"),
                                HeartRate = GetProperty<float?>(d, "HeartRate"),
                                DeviceName = GetProperty<string>(d, "DeviceName")
                            }).ToList()
                        };
                    }
                case "bloodpressure":
                    {
                        var validData = groupData.Where(d =>
                            GetProperty<float?>(d, "SystolicPressure") != null ||
                            GetProperty<float?>(d, "DiastolicPressure") != null).ToList();

                        if (!validData.Any()) return new { Label = label, Count = 0 };

                        var systolicData = validData.Where(d => GetProperty<float?>(d, "SystolicPressure") != null).ToList();
                        var diastolicData = validData.Where(d => GetProperty<float?>(d, "DiastolicPressure") != null).ToList();

                        return new
                        {
                            Label = label,
                            Count = validData.Count,
                            SystolicAvg = systolicData.Any() ?
                                Math.Round(systolicData.Average(d => GetProperty<float?>(d, "SystolicPressure").Value), 1) :
                                (double?)null,
                            DiastolicAvg = diastolicData.Any() ?
                                Math.Round(diastolicData.Average(d => GetProperty<float?>(d, "DiastolicPressure").Value), 1) :
                                (double?)null,
                            Data = validData.Select(d => new
                            {
                                RecordedAt = GetProperty<DateTime>(d, "RecordedAt"),
                                SystolicPressure = GetProperty<float?>(d, "SystolicPressure"),
                                DiastolicPressure = GetProperty<float?>(d, "DiastolicPressure"),
                                DeviceName = GetProperty<string>(d, "DeviceName")
                            }).ToList()
                        };
                    }
                case "temperature":
                    {
                        var validData = groupData.Where(d => GetProperty<float?>(d, "Temperature") != null).ToList();
                        if (!validData.Any()) return new { Label = label, Count = 0 };

                        return new
                        {
                            Label = label,
                            Count = validData.Count,
                            Min = validData.Min(d => GetProperty<float?>(d, "Temperature").Value),
                            Max = validData.Max(d => GetProperty<float?>(d, "Temperature").Value),
                            Average = Math.Round(validData.Average(d => GetProperty<float?>(d, "Temperature").Value), 1),
                            Data = validData.Select(d => new
                            {
                                RecordedAt = GetProperty<DateTime>(d, "RecordedAt"),
                                Temperature = GetProperty<float?>(d, "Temperature"),
                                DeviceName = GetProperty<string>(d, "DeviceName")
                            }).ToList()
                        };
                    }
                case "spo2":
                    {
                        var validData = groupData.Where(d => GetProperty<float?>(d, "SpO2") != null).ToList();
                        if (!validData.Any()) return new { Label = label, Count = 0 };

                        return new
                        {
                            Label = label,
                            Count = validData.Count,
                            Min = validData.Min(d => GetProperty<float?>(d, "SpO2").Value),
                            Max = validData.Max(d => GetProperty<float?>(d, "SpO2").Value),
                            Average = Math.Round(validData.Average(d => GetProperty<float?>(d, "SpO2").Value), 1),
                            Data = validData.Select(d => new
                            {
                                RecordedAt = GetProperty<DateTime>(d, "RecordedAt"),
                                SpO2 = GetProperty<float?>(d, "SpO2"),
                                DeviceName = GetProperty<string>(d, "DeviceName")
                            }).ToList()
                        };
                    }
                case "bloodph":
                    {
                        var validData = groupData.Where(d => GetProperty<float?>(d, "BloodPh") != null).ToList();
                        if (!validData.Any()) return new { Label = label, Count = 0 };

                        return new
                        {
                            Label = label,
                            Count = validData.Count,
                            Min = validData.Min(d => GetProperty<float?>(d, "BloodPh").Value),
                            Max = validData.Max(d => GetProperty<float?>(d, "BloodPh").Value),
                            Average = Math.Round(validData.Average(d => GetProperty<float?>(d, "BloodPh").Value), 2),
                            Data = validData.Select(d => new
                            {
                                RecordedAt = GetProperty<DateTime>(d, "RecordedAt"),
                                BloodPh = GetProperty<float?>(d, "BloodPh"),
                                DeviceName = GetProperty<string>(d, "DeviceName")
                            }).ToList()
                        };
                    }
                default:
                    return new { Label = label, Count = 0 };
            }
        }

       
        private TValue GetProperty<TValue>(object obj, string propertyName)
        {
            if (obj == null) return default;

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default;

          
            var value = property.GetValue(obj);
            if (value == null) return default;

           
            try
            {
                return (TValue)value;
            }
            catch
            {
                return default;
            }
        }
    }
}

