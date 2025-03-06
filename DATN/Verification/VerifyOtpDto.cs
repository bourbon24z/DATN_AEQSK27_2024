namespace DATN.Verification
{
    public class VerifyOtpDto 
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string PatientName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string Phone { get; set; }
    }
}
