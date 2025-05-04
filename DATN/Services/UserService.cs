using DATN.Data;
using DATN.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services
{
    public class UserService : IUserService
    {
        private readonly StrokeDbContext _context;

        public UserService(StrokeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StrokeUser>> GetAllUsersAsync(string? role = null)
        {
            if (string.IsNullOrEmpty(role))
            {
                return await _context.StrokeUsers.AsNoTracking().ToListAsync();
            }

            var userIdsInRole = await _context.UserRoles
                .Where(ur => ur.Role.RoleName == role && ur.IsActive)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.StrokeUsers
                .AsNoTracking()
                .Where(u => userIdsInRole.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task<IEnumerable<StrokeUser>> GetPatientsAsync()
        {
            var patientRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "user");

            if (patientRole == null)
                return Enumerable.Empty<StrokeUser>();

            var patientIds = await _context.UserRoles
                .Where(ur => ur.RoleId == patientRole.RoleId && ur.IsActive)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.StrokeUsers
                .AsNoTracking()
                .Where(u => patientIds.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task<IEnumerable<StrokeUser>> GetDoctorsAsync()
        {
            var doctorRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "doctor");

            if (doctorRole == null)
                return Enumerable.Empty<StrokeUser>();

            var doctorIds = await _context.UserRoles
                .Where(ur => ur.RoleId == doctorRole.RoleId && ur.IsActive)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.StrokeUsers
                .AsNoTracking()
                .Where(u => doctorIds.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task<IEnumerable<StrokeUser>> GetAdminsAsync()
        {
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "admin");

            if (adminRole == null)
                return Enumerable.Empty<StrokeUser>();

            var adminIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRole.RoleId && ur.IsActive)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.StrokeUsers
                .AsNoTracking()
                .Where(u => adminIds.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task<StrokeUser?> GetUserByIdAsync(int userId)
        {
            return await _context.StrokeUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur =>
                    ur.UserId == userId &&
                    ur.Role.RoleName == roleName &&
                    ur.IsActive);
        }

        public async Task<bool> CanAccessUserDataAsync(int requestingUserId, int targetUserId)
        {
            
            if (requestingUserId == targetUserId)
                return true;

            
            var isAdmin = await HasRoleAsync(requestingUserId, "admin");
            if (isAdmin)
                return true;


            var isDoctor = await HasRoleAsync(requestingUserId, "doctor");
            if (isDoctor)
            {
                var isTargetPatient = await HasRoleAsync(targetUserId, "user");
                if (!isTargetPatient)
                    return false;

                return await _context.Relationships
                    .AnyAsync(r =>
                        r.InviterId == requestingUserId &&
                        r.UserId == targetUserId &&
                        r.RelationshipType == "doctor-patient");
            }


            return false;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }
    }
}