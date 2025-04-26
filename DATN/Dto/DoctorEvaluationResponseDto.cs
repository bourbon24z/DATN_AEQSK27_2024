namespace DATN.Dto
{
    public class DoctorEvaluationResponseDto
    {
        public int DoctorEvaluationId { get; set; }  
        public DateTime EvaluationDate { get; set; }
        public string EvaluationNotes { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int CaseHistoryId { get; set; }
    }
}
