using DATN.Models;

namespace DATN.Services
{
    public interface IUserService
    {
        Task<IEnumerable<StrokeUser>> GetAllUsersAsync(string? role = null);
        Task<IEnumerable<StrokeUser>> GetPatientsAsync();
        Task<IEnumerable<StrokeUser>> GetDoctorsAsync();
        Task<IEnumerable<StrokeUser>> GetAdminsAsync();
        Task<StrokeUser?> GetUserByIdAsync(int userId);
        Task<bool> HasRoleAsync(int userId, string roleName);
        Task<bool> CanAccessUserDataAsync(int requestingUserId, int targetUserId);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    }
}