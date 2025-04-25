using System;

namespace DATN.Dto
{
    public class DeviceDataDto
    {
        public int UserId { get; set; }
        public int? DeviceId { get; set; }
        public MeasurementDataDto Measurements { get; set; }
        public GPSDataDto GPS { get; set; }  
        public DateTime? RecordedAt { get; set; }
    }

   
    public class MeasurementDataDto
    {
        public float? Temperature { get; set; }          // °C, chuẩn 37°C
        public float? HeartRate { get; set; }              // bpm, chuẩn 75 bpm
        public float? SystolicPressure { get; set; }       // mmHg, chuẩn 120 mmHg
        public float? DiastolicPressure { get; set; }      // mmHg, chuẩn 80 mmHg
        public float? SPO2 { get; set; }                   // %, chuẩn 95%
        public float? BloodPH { get; set; }                // pH, chuẩn 7.4
    }

    public class GPSDataDto
    {
        public float Lat { get; set; }
        public float Long { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
