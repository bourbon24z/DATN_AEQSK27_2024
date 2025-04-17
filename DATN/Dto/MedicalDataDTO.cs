namespace DATN.Dto
{
	public class MedicalDataDTO
	{
		public string Series { get; set; }
		public float? SystolicPressure { get; set; }
		public float? DiastolicPressure { get; set; }
		public float? Temperature { get; set; }
		public float? BloodPh { get; set; }
		public DateTime RecordedAt { get; set; }
		public float? Spo2Information { get; set; }
		public float? HeartRate { get; set; }
	}
}
