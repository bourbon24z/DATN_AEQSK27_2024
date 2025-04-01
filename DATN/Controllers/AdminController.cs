using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DATN.Data;
using DATN.Models;
using DATN.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AdminController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        public AdminController(StrokeDbContext context)
        {
            _context = context;
        }
        /// Create a new admin account
        [HttpPost("create-admin")]
        //http://localhost:5062/api/admin/create-admin
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto model)
        {
            try
            {
                // Check if username exists
                if (await _context.StrokeUsers.AnyAsync(u => u.Username == model.Username))
                {
                    return BadRequest($"The username '{model.Username}' already exists.");
                }

                // Create new admin user
                var newAdminUser = new StrokeUser
                {
                    Username = model.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    PatientName = model.PatientName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Email = model.Email,
                    CreatedAt = DateTime.UtcNow,
                    IsVerified = true
                };
                _context.StrokeUsers.Add(newAdminUser);
                await _context.SaveChangesAsync();

                // Ensure admin role exists
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin")
                                ?? new Role { RoleName = "admin" };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();

                // Assign admin role to user
                _context.UserRoles.Add(new UserRole
                {
                    UserId = newAdminUser.UserId,
                    RoleId = adminRole.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await _context.SaveChangesAsync();

                return Ok($"The admin account '{model.Username}' has been successfully created.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        // Assign admin role to an existing user
        [HttpPost("add-admin-role")]
        //http://localhost:5062/api/admin/add-admin-role
        public async Task<IActionResult> AddAdminRole([FromBody] AddAdminRoleDto model)
        {
            try
            {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                if (user == null) return NotFound($"The user with UserId {model.UserId} does not exist.");

                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin")
                                ?? new Role { RoleName = "admin" };
                if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == model.UserId && ur.RoleId == adminRole.RoleId))
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = model.UserId,
                        RoleId = adminRole.RoleId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    await _context.SaveChangesAsync();

                    return Ok($"The admin role has been assigned to user {model.UserId}.");
                }

                return BadRequest("The user already has the admin role.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Update admin role status for a user
        [HttpPost("update-admin-status")]
        //http://localhost:5062/api/admin/update-admin-status
        public async Task<IActionResult> UpdateAdminStatus(int userId, bool isActive)
        {
            try
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "admin");
                if (adminRole == null) return NotFound("The admin role does not exist.");

                var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.RoleId);
                if (userRole == null) return NotFound("The user does not have the admin role.");

                userRole.IsActive = isActive;
                await _context.SaveChangesAsync();

                return Ok($"The admin role status for user {userId} has been updated to {isActive}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        // Get roles of a specific user
        [HttpGet("user-roles/{userId}")]
        //http://localhost:5062/api/admin/user-roles/{userId}
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            try
            {
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("users")]
        //http://localhost:5062/api/admin/users
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.StrokeUsers.AsNoTracking().ToListAsync();

                var userDtos = new List<object>();
                foreach (var user in users)
                {
                    var roles = await _context.UserRoles.AsNoTracking()
                        .Where(ur => ur.UserId == user.UserId && ur.IsActive)
                        .Select(ur => ur.Role.RoleName)
                        .ToListAsync();

                    userDtos.Add(new
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

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpDelete("remove-admin-role/{userId}")]
        //http://localhost:5062/api/admin/remove-admin-role/{userId}
        public async Task<IActionResult> RemoveAdminRole(int userId)
        {
            try
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "admin");
                if (adminRole == null) return NotFound("The admin role does not exist.");

                var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.RoleId);
                if (userRole == null) return NotFound("The user does not have the admin role.");

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();

                return Ok($"The admin role of user {userId} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        // Delete a user and all related roles    
        [HttpDelete("delete-user/{userId}")]
        //http://localhost:5062/api/admin/delete-user/{userId}
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null) return NotFound($"The user with UserId {userId} does not exist.");

                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
                if (userRoles.Any())
                {
                    _context.UserRoles.RemoveRange(userRoles);
                }

                _context.StrokeUsers.Remove(user);
                await _context.SaveChangesAsync();

                return Ok($"The user with UserId {userId} has been successfully deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
