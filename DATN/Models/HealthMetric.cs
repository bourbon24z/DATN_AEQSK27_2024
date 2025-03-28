using DATN.Models;

public class HealthMetric
{
    public int HealthMetricId { get; set; }
    public int UserId { get; set; }
    public int DeviceId { get; set; }
    public float SystolicPressure { get; set; }
    public float DiastolicPressure { get; set; }
    public float Temperature { get; set; }
    public float BloodPh { get; set; }
    public DateTime RecordedAt { get; set; }
    public StrokeUser StrokeUser { get; set; } 
    public Device Device { get; set; } 
}
