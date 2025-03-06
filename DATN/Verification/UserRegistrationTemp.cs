namespace DATN.Verification
{
    //cache user
    public class UserRegistrationTemp
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
