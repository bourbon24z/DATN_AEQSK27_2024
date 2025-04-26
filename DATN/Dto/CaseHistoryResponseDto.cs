namespace DATN.Dto
{
    public class CaseHistoryResponseDto
    {
        public int CaseHistoryId { get; set; }
        public string ProgressNotes { get; set; }
        public DateTime Time { get; set; }
        public string StatusOfMr { get; set; }
        public int UserId { get; set; }
        public string PatientName { get; set; }
        public string FormattedTime => Time.ToString("dd/MM/yyyy HH:mm");
    }
}
