namespace DATN.Dto
{
    public class PatientDto
    {
        public string PatientName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
