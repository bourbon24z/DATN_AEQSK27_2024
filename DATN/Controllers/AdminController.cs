using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DATN.Data;
using DATN.Dto;
using DATN.Models;

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

        
        private async Task<Role> GetOrCreateAdminRoleAsync()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
            if (adminRole == null)
            {
                adminRole = new Role { RoleName = "admin" };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();
            }
            return adminRole;
        }

        private async Task<List<string>> ValidateDuplicateAdminAsync(CreateAdminDto dto)
        {
            var duplicateUser = await _context.StrokeUsers
                .FirstOrDefaultAsync(u =>
                    u.Username == dto.Username ||
                    u.Email == dto.Email ||
                    u.Phone == dto.Phone);

            var errors = new List<string>();
            if (duplicateUser != null)
            {
                if (duplicateUser.Username == dto.Username)
                    errors.Add($"The username '{dto.Username}' already exists.");
                if (duplicateUser.Email == dto.Email)
                    errors.Add($"The email '{dto.Email}' already exists.");
                if (duplicateUser.Phone == dto.Phone)
                    errors.Add($"The phone number '{dto.Phone}' already exists.");
            }
            return errors;
        }

        [HttpPost("create-admin")]
        //http://localhost:5062/api/admin/create-admin
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto model)
        {
            try
            {
               
                var duplicateErrors = await ValidateDuplicateAdminAsync(model);
                if (duplicateErrors.Any())
                    return BadRequest(string.Join(" ", duplicateErrors));

                
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

                
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        _context.StrokeUsers.Add(newAdminUser);
                        await _context.SaveChangesAsync();

                        
                        var adminRole = await _context.Roles
                            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
                        if (adminRole == null)
                        {
                            
                            throw new Exception("The 'admin' role does not exist in the database.");
                        }

                       
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = newAdminUser.UserId,
                            RoleId = adminRole.RoleId,
                            CreatedAt = DateTime.Now,  
                            IsActive = true
                        });
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                });

                return Ok($"The admin account '{model.Username}' has been successfully created.");
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpPost("add-admin-role")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/add-admin-role
        public async Task<IActionResult> AddAdminRole([FromBody] AddAdminRoleDto model)
        {
            try
            {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                if (user == null)
                    return NotFound($"The user with UserId {model.UserId} does not exist.");

                var adminRole = await GetOrCreateAdminRoleAsync();
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

      
        [HttpPost("update-admin-status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateAdminStatus(int userId, bool isActive)
        {
            try
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
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

       
        [HttpGet("user-roles/{userId}")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/user-roles/13
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/remove-admin-role/13
        public async Task<IActionResult> RemoveAdminRole(int userId)
        {
            try
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
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

    
        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/delete-user/13
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
