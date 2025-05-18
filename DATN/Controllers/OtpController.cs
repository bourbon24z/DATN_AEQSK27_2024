using DATN.Data;
using DATN.Dto;
using DATN.Models;
using DATN.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly StrokeDbContext _context;
    private readonly EmailService _emailService;

    public OtpController(StrokeDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    [NonAction]
    public async Task CreateAndSendOtpAsync(int userId, string email, string verificationType)
    {
        
        var otp = new Random().Next(100000, 999999).ToString();

        
        var userVerification = new UserVerification
        {
            UserId = userId,
            Email = email,
            VerificationCode = otp,
            OtpExpiry = DateTime.Now.AddMinutes(15), 
            IsVerified = false 
        };
        _context.UserVerifications.Add(userVerification);
        await _context.SaveChangesAsync();

        // send otp to mail
        await _emailService.SendEmailAsync(email, $"{verificationType} Confirmation",
            $"Your OTP is: {otp}. It will expire in 15 minutes.");
    }
    [HttpPost("send-otp")]
    [Authorize]
    //http://localhost:5062/api/Otp/send-otp
    public async Task<IActionResult> SendOtp()
    {
       
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        var dbUser = await _context.StrokeUsers.FindAsync(userId);

        if (dbUser == null)
        {
            return NotFound("User not found.");
        }

        var otp = new Random().Next(100000, 999999).ToString();

       
        var userVerification = new UserVerification
        {
            UserId = dbUser.UserId,
            Email = dbUser.Email,
            VerificationCode = otp,
            OtpExpiry = DateTime.Now.AddMinutes(15), 
            IsVerified = false
        };
        _context.UserVerifications.Add(userVerification);
        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
                dbUser.Email,
                "OTP đây ní ơi",
                $"Gửi ní {dbUser.PatientName},\n\n" +
                $"Otp của ní: {otp}. 15 phút thôi ní nghe.\n\n" +
                "NÍ không gửi thì bỏ qua hoặc đổi mật khẩu đê.\n\n" +
                "Trân trọng,\nFrom Huy Nguyễn Cute With Love."
);


        return Ok("OTP has been sent to your registered email for verification.");
    }
    [HttpPut("verify-and-update")]
    [Authorize]
    public async Task<IActionResult> VerifyAndUpdateContact([FromBody] UpdateContactWithOtpDto updateDto)
    {
        
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        var dbUser = await _context.StrokeUsers.FindAsync(userId);

        if (dbUser == null)
        {
            return NotFound("User not found.");
        }

        // check otp from userverification
        var verification = await _context.UserVerifications
            .Where(v => v.UserId == dbUser.UserId && v.Email == dbUser.Email && v.VerificationCode == updateDto.Otp)
            .OrderByDescending(v => v.OtpExpiry)
            .FirstOrDefaultAsync();

        if (verification == null || verification.OtpExpiry < DateTime.Now)
        {
            return BadRequest("Invalid or expired OTP.");
        }

        // put new email
        if (!string.IsNullOrEmpty(updateDto.NewEmail))
        {
            dbUser.Email = updateDto.NewEmail;
        }

        // put new phone
        if (!string.IsNullOrEmpty(updateDto.NewPhone))
        {
            dbUser.Phone = updateDto.NewPhone;
        }

        await _context.SaveChangesAsync();

        // mark otp has been checked
        verification.IsVerified = true;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Contact information updated successfully.",
            data = new
            {
                dbUser.Email,
                dbUser.Phone
            }
        });
    }




}


