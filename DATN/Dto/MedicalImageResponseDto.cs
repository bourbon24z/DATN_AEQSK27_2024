namespace DATN.Dto
{
    public class MedicalImageResponseDto
    {
        public int MedicalImageId { get; set; } 
        public string ImageUrl { get; set; }
        public DateTime CapturedAt { get; set; }
        public string Metadata { get; set; }
        public int CaseHistoryId { get; set; }
    }
}
