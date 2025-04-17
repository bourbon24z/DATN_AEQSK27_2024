using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DATN.Models
{
    public class Device
    {
       
            public int DeviceId { get; set; }
		    public int UserId { get; set; }
		    public string DeviceName { get; set; }
            public string DeviceType { get; set; }
            public string Series { get; set; }

        public ICollection<UserMedicalData> UserMedicalDatas { get; set; } = new List<UserMedicalData>();
        [JsonIgnore]
		public StrokeUser User { get; set; }
	}
}
