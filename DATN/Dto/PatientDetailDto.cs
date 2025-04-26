namespace DATN.Dto
{
    public class PatientDetailDto
    {
        public int UserId { get; set; }
        public string PatientName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string GenderText => Gender ? "Nam" : "Nữ";
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
