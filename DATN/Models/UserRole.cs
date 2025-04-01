namespace DATN.Models
{
    public class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public StrokeUser StrokeUser { get; set; }
        public Role Role { get; set; }
    }
}
