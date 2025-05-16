namespace DATN.Dto
{
	public class deviceDTO
	{
		public int DeviceId { get; set; }
        public int UserId { get; set; }
		public string DeviceName { get; set; }
		public string DeviceType { get; set; }
		public string Series { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
