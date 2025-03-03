namespace DATN.Dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public PatientDto Patient { get; set; }
    }
}
