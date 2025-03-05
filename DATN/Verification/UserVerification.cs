namespace DATN.Verification
{
    public class UserVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string VerificationCode { get; set; }
        public bool IsVerified { get; set; }
    }
}
