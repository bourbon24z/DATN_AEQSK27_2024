namespace DATN.Dto
{
    public class UserGpsDto
    {
       public int UserId { get; set; }
       public float Lat { get; set; }
		public float Long { get; set; }


        public int? DeviceId { get; set; }
        public float? ReadingValue { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}
