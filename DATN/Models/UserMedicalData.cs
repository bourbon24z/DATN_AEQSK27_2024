namespace DATN.Models
{
    public class UserMedicalData
    {
        public int UserMedicalDataId { get; set; }
        public int UserId { get; set; }
        public int? DeviceId { get; set; }  
        public float? SystolicPressure { get; set; }
        public float? DiastolicPressure { get; set; }
        public float? Temperature { get; set; }
        public float? BloodPh { get; set; }
        public DateTime? RecordedAt { get; set; }
        public float? Spo2Information { get; set; }
        public float? HeartRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public StrokeUser User { get; set; }
        public Device Device { get; set; }
    }

}
