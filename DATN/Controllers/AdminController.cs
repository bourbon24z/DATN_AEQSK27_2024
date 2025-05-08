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
using System.Security.Claims;

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

                
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "user");

                if (adminRole == null)
                    return NotFound("Admin role not found in the system.");

                var hasAdminRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == model.UserId && ur.RoleId == adminRole.RoleId && ur.IsActive);

                if (hasAdminRole)
                    return BadRequest("The user already has the admin role.");

                
                if (userRole != null)
                {
                    var userRoleAssignment = await _context.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == model.UserId && ur.RoleId == userRole.RoleId && ur.IsActive);

                    if (userRoleAssignment != null)
                    {
                        userRoleAssignment.IsActive = false;
                        // _context.UserRoles.Remove(userRoleAssignment);
                    }
                }

               
                _context.UserRoles.Add(new UserRole
                {
                    UserId = model.UserId,
                    RoleId = adminRole.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });

                await _context.SaveChangesAsync();
                return Ok($"The admin role has been assigned to user {model.UserId} and user role has been removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("add-doctor-role/{userId}")]
        //http://localhost:5062/api/admin/add-doctor-role
        public async Task<IActionResult> AddDoctorRole(int userId)
        {
            try
            {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound(new { message = $"User with ID {userId} does not exist." });

                var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "doctor");
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
                if (doctorRole == null || userRole == null)
                    return NotFound(new { message = "The role 'doctor' or 'user' does not exist." });

                var hasDoctor = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == doctorRole.RoleId && ur.IsActive);
                if (hasDoctor)
                    return BadRequest(new { message = $"User {userId} already has the 'doctor' role." });

                
                var userRoleEntity = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == userRole.RoleId && ur.IsActive);
                if (userRoleEntity != null)
                {
                    userRoleEntity.IsActive = false;
                    // _context.UserRoles.Remove(userRoleEntity); // del thủ công
                }

                
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = doctorRole.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
                return Ok(new { message = $"The role 'doctor' has been assigned to user {userId}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

       
        [HttpDelete("remove-doctor-role/{userId}")]
        //http://localhost:5062/api/admin/remove-doctor-role
        public async Task<IActionResult> RemoveDoctorRole(int userId)
        {
            try
            {
                var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "doctor");
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
                if (doctorRole == null || userRole == null)
                    return NotFound(new { message = "Role 'doctor' or 'user' does not exitst." });

                var doctorRoleEntity = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == doctorRole.RoleId && ur.IsActive);
                if (doctorRoleEntity == null)
                    return NotFound(new { message = $"'Doctor' role does not exist on User {userId}." });

                
                doctorRoleEntity.IsActive = false;
                // _context.UserRoles.Remove(doctorRoleEntity); // del thủ công

              
                var hasUserRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == userRole.RoleId && ur.IsActive);
                if (!hasUserRole)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = userId,
                        RoleId = userRole.RoleId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $" 'doctor' Role has been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
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
        public async Task<IActionResult> GetUserRoles(int userId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                IQueryable<UserRole> query = _context.UserRoles.Where(ur => ur.UserId == userId);

                if (!includeInactive)
                {
                    query = query.Where(ur => ur.IsActive);
                }

                var result = await query
                    .Include(ur => ur.Role)
                    .Select(ur => new
                    {
                        RoleName = ur.Role.RoleName,
                        IsActive = ur.IsActive
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //[HttpGet("users")]
        //[Authorize(Roles = "admin")]
        ////http://localhost:5062/api/admin/users
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    try
        //    {
        //        var users = await _context.StrokeUsers.AsNoTracking().ToListAsync();
        //        var userDtos = new List<object>();

        //        foreach (var user in users)
        //        {
        //            var roles = await _context.UserRoles.AsNoTracking()
        //                .Where(ur => ur.UserId == user.UserId && ur.IsActive)
        //                .Select(ur => ur.Role.RoleName)
        //                .ToListAsync();

        //            userDtos.Add(new
        //            {
        //                UserId = user.UserId,
        //                Username = user.Username,
        //                Roles = roles,
        //                PatientName = user.PatientName,
        //                DateOfBirth = user.DateOfBirth,
        //                Gender = user.Gender,
        //                Phone = user.Phone,
        //                Email = user.Email
        //            });
        //        }
        //        return Ok(userDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}
        [HttpGet("users")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/users
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserRoles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(currentUserIdStr, out int currentUserId))
                {
                    return BadRequest("Invalid user identifier");
                }

                IQueryable<StrokeUser> usersQuery = _context.StrokeUsers.AsNoTracking();

                if (!currentUserRoles.Contains("admin"))
                {
                    var patientUserIds = await _context.UserRoles
                        .Where(ur => ur.Role.RoleName == "user" && ur.IsActive)
                        .Select(ur => ur.UserId)
                        .Distinct()
                        .ToListAsync();

                    usersQuery = usersQuery.Where(u => patientUserIds.Contains(u.UserId));
                }
                else if (!string.IsNullOrEmpty(role))
                {
                    var filteredUserIds = await _context.UserRoles
                        .Where(ur => ur.Role.RoleName == role && ur.IsActive)
                        .Select(ur => ur.UserId)
                        .Distinct()
                        .ToListAsync();

                    usersQuery = usersQuery.Where(u => filteredUserIds.Contains(u.UserId));
                }

                var totalCount = await usersQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedUsers = await usersQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userIds = paginatedUsers.Select(u => u.UserId).ToList();

                
                var allUserRoles = await _context.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId) && ur.IsActive)
                    .Select(ur => new { ur.UserId, RoleName = ur.Role.RoleName })
                    .ToListAsync();

                
                var userRoleCounts = await _context.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId))
                    .GroupBy(ur => ur.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalRoles = g.Count(),
                        ActiveRoles = g.Count(ur => ur.IsActive)
                    })
                    .ToListAsync();

                var userDtos = paginatedUsers.Select(user =>
                {
                    
                    var roleInfo = userRoleCounts.FirstOrDefault(rc => rc.UserId == user.UserId);
                    bool isLocked = roleInfo != null && roleInfo.ActiveRoles == 0;

                    return new
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Roles = allUserRoles
                            .Where(ur => ur.UserId == user.UserId)
                            .Select(ur => ur.RoleName)
                            .ToList(),
                        PatientName = user.PatientName,
                        DateOfBirth = user.DateOfBirth,
                        Gender = user.Gender,
                        Phone = user.Phone,
                        Email = user.Email,
                        AccountStatus = isLocked ? "Locked" : "Active",
                        IsLocked = isLocked
                    };
                }).ToList();

                return Ok(new
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Users = userDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("users/by-role/{roleName}")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/users/by-role/doctor
        public async Task<IActionResult> GetUsersByRole(string roleName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                    return BadRequest($"Role '{roleName}' does not exist.");

                
                var userIdsWithRole = await _context.UserRoles
                    .Where(ur => ur.Role.RoleName == roleName && ur.IsActive)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                
                var usersQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => userIdsWithRole.Contains(u.UserId));

                
                var totalCount = await usersQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedUsers = await usersQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userIds = paginatedUsers.Select(u => u.UserId).ToList();
                var allUserRoles = await _context.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId) && ur.IsActive)
                    .Select(ur => new { ur.UserId, RoleName = ur.Role.RoleName })
                    .ToListAsync();

                var userDtos = paginatedUsers.Select(user => new
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Roles = allUserRoles
                        .Where(ur => ur.UserId == user.UserId)
                        .Select(ur => ur.RoleName)
                        .ToList(),
                    PatientName = user.PatientName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                }).ToList();

                return Ok(new
                {
                    Role = roleName,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Users = userDtos
                });
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
        [HttpPost("remove-admin/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveAdminFromUser(int userId)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentUserIdStr, out int currentUserId) && userId == currentUserId)
                {
                    return BadRequest("You cannot remove your own admin privileges.");
                }

                
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "admin");
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "user");

                if (adminRole == null)
                    return NotFound("Admin role not found in the system.");
                if (userRole == null)
                    return NotFound("User role not found in the system.");

                
                var adminRoleAssignment = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.RoleId && ur.IsActive);

                if (adminRoleAssignment == null)
                    return NotFound("The user does not have an admin role.");

                
                adminRoleAssignment.IsActive = false;
                //_context.UserRoles.Remove(adminRoleAssignment);

                
                var hasUserRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == userRole.RoleId && ur.IsActive);

                
                if (!hasUserRole)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = userId,
                        RoleId = userRole.RoleId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                return Ok($"Admin role has been removed from user {userId} and user role has been assigned as default.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("doctor-patients/{doctorId}")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/doctor-patients/123
        public async Task<IActionResult> GetDoctorPatients(int doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                
                var doctor = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == doctorId);
                if (doctor == null)
                    return NotFound($"Doctor with ID {doctorId} not found");

                
                var isDoctorRole = await _context.UserRoles
                    .AnyAsync(ur =>
                        ur.UserId == doctorId &&
                        ur.Role.RoleName == "doctor" &&
                        ur.IsActive);

                if (!isDoctorRole)
                    return BadRequest($"User with ID {doctorId} is not a doctor");

                
                var patientIds = await _context.Relationships
                    .Where(r => r.InviterId == doctorId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.UserId)
                    .ToListAsync();

                if (!patientIds.Any())
                {
                    return Ok(new
                    {
                        DoctorId = doctorId,
                        DoctorName = doctor.PatientName,
                        TotalCount = 0,
                        TotalPages = 0,
                        CurrentPage = page,
                        PageSize = pageSize,
                        Patients = new List<object>()
                    });
                }

                
                var patientsQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => patientIds.Contains(u.UserId));

                var totalCount = await patientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var patients = await patientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var patientDtos = patients.Select(patient => new
                {
                    UserId = patient.UserId,
                    Username = patient.Username,
                    PatientName = patient.PatientName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Email = patient.Email
                }).ToList();

                return Ok(new
                {
                    DoctorId = doctorId,
                    DoctorName = doctor.PatientName,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Patients = patientDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost("toggle-account-status")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/toggle-account-status
        public async Task<IActionResult> ToggleAccountStatus([FromBody] ToggleAccountStatusDto model)
        {
            try
            {
                if (model == null || !model.UserId.HasValue)
                    return BadRequest("Yêu cầu phải có ID người dùng");

                int userId = model.UserId.Value;
                bool activateAccount = model.Activate ?? false;// default is false

               
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentUserIdStr, out int currentUserId) && userId == currentUserId)
                {
                    return BadRequest("Bạn không thể khóa tài khoản của chính mình");
                }

                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound($"Không tìm thấy người dùng có ID {userId}");

                
                var userRolesWithRoleInfo = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();

                if (!userRolesWithRoleInfo.Any())
                    return NotFound($"Người dùng {userId} không có role nào");

               
                if (!activateAccount)
                {
                    var isAdmin = userRolesWithRoleInfo.Any(ur => ur.Role.RoleName.ToLower() == "admin" && ur.IsActive);
                    if (isAdmin)
                    {
                        var adminRoleId = userRolesWithRoleInfo.First(ur => ur.Role.RoleName.ToLower() == "admin").RoleId;

                        var activeAdminCount = await _context.UserRoles
                            .CountAsync(ur => ur.RoleId == adminRoleId && ur.IsActive && ur.UserId != userId);

                        if (activeAdminCount < 1)
                        {
                            return BadRequest("Không thể khóa tài khoản admin cuối cùng. Hãy tạo admin khác trước.");
                        }
                    }
                }

                var previousRoleStatus = userRolesWithRoleInfo
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.Role.RoleName)
                    .Distinct()
                    .ToList();

                if (activateAccount)
                {
                   
                    var distinctRoleNames = userRolesWithRoleInfo
                        .Select(ur => ur.Role.RoleName)
                        .Distinct()
                        .ToList();

                    foreach (var roleName in distinctRoleNames)
                    {
                        
                        var roleToActivate = userRolesWithRoleInfo
                            .Where(ur => ur.Role.RoleName == roleName)
                            .OrderBy(ur => ur.UserRoleId)
                            .FirstOrDefault();

                        if (roleToActivate != null)
                        {
                            roleToActivate.IsActive = true;

                            
                            foreach (var duplicateRole in userRolesWithRoleInfo.Where(ur =>
                                ur.Role.RoleName == roleName && ur.UserRoleId != roleToActivate.UserRoleId))
                            {
                                duplicateRole.IsActive = false;
                            }
                        }
                    }
                }
                else
                {
                    
                    foreach (var role in userRolesWithRoleInfo)
                    {
                        role.IsActive = false;
                    }
                }

                await _context.SaveChangesAsync();

                // Lấy danh sách role sau khi cập nhật
                var updatedRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.RoleId,
                        (ur, r) => r.RoleName)
                    .Distinct()
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = activateAccount
                        ? $"Đã mở khóa tài khoản cho người dùng {userId}"
                        : $"Đã khóa tài khoản của người dùng {userId}",
                    user = new
                    {
                        userId = user.UserId,
                        username = user.Username,
                        patientName = user.PatientName,
                        email = user.Email,
                        status = activateAccount ? "Hoạt động" : "Đã khóa",
                        previousRoles = previousRoleStatus,
                        currentRoles = updatedRoles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpGet("relationships")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/admin/relationships?type=doctor-patient
        public async Task<IActionResult> GetAllRelationships([FromQuery] string type = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                IQueryable<Relationship> query = _context.Relationships
                    .Include(r => r.User)
                    .Include(r => r.Inviter)
                    .AsNoTracking();

                
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(r => r.RelationshipType == type);
                }

                
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var relationships = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var relationshipDtos = relationships.Select(r => new
                {
                    RelationshipId = r.RelationshipId,
                    RelationshipType = r.RelationshipType,
                    CreatedAt = r.CreatedAt,
                    User = new
                    {
                        UserId = r.User.UserId,
                        Username = r.User.Username,
                        Name = r.User.PatientName,
                        Email = r.User.Email
                    },
                    Inviter = new
                    {
                        UserId = r.Inviter.UserId,
                        Username = r.Inviter.Username,
                        Name = r.Inviter.PatientName,
                        Email = r.Inviter.Email
                    }
                }).ToList();

                return Ok(new
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Relationships = relationshipDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
