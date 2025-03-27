//using DATN.Data;
//using DATN.Dto;
//using DATN.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//[ApiController]
//[Route("api/[controller]")]
//public class EmailController : ControllerBase
//{
//    private readonly EmailService _emailService;
//    private readonly StrokeDbContext _context;

//    public EmailController(EmailService emailService, StrokeDbContext context)
//    {
//        _emailService = emailService;
//        _context = context;
//    }

//    [HttpPost("send-otp")]
//    //http://localhost:5062/api/Email/send-otp
//    public async Task<IActionResult> SendOtp([FromBody] RegisterUserDto registerUserDto)
//    {
//        var existingUser = await _context.StrokeUsers.SingleOrDefaultAsync(u =>
//            u.Email == registerUserDto.Email ||
//            u.Phone == registerUserDto.Phone ||
//            u.Username == registerUserDto.Username);
//        var tempExists = await _context.UserRegistrationTemps.AnyAsync(u =>
//            u.Email == registerUserDto.Email ||
//            u.Phone == registerUserDto.Phone ||
//            u.Username == registerUserDto.Username);

//        if (existingUser != null || tempExists)
//        {
//            return BadRequest("Email, phone number or username already exists.");
//        }

//        var tempUser = new UserRegistrationTemp
//        {
//            Username = registerUserDto.Username,
//            Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
//            Role = registerUserDto.Role,
//            Email = registerUserDto.Email,
//            Otp = new Random().Next(100000, 999999).ToString(),
//            OtpExpiry = DateTime.UtcNow.AddMinutes(15),
//            PatientName = registerUserDto.PatientName,
//            DateOfBirth = registerUserDto.DateOfBirth,
//            Gender = registerUserDto.Gender,
//            Phone = registerUserDto.Phone
//        };

//        _context.UserRegistrationTemps.Add(tempUser);
//        await _context.SaveChangesAsync();

//        await _emailService.SendEmailAsync(registerUserDto.Email, "OTP Confirmation", $"Your OTP is: {tempUser.Otp}");

//        return Ok("OTP has been sent to your email. Please verify.");
//    }
//}
