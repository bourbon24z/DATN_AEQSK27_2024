using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN.Configuration;
using DATN.Dto;
using DATN.Models;
using DATN.Services;
using BCrypt.Net;
using DATN.Data;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        public AuthController(StrokeDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            
            var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.Username == loginDto.Credential);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

          
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = _jwtTokenService.GenerateToken(user);

            
            var roles = await _context.UserRoles
                            .Where(ur => ur.UserId == user.UserId && ur.IsActive)
                            .Select(ur => ur.Role.RoleName)
                            .ToListAsync();

            return Ok(new
            {
                message = "Login Successfully.",
                data = new
                {
                    userId = user.UserId,
                    username = user.Username,
                    dateOfBirth = user.DateOfBirth,
                    email = user.Email,
                    gender = user.Gender,
                    phone = user.Phone,
                    roles,
                    token
                }
            });
        }
    }
}
