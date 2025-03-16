using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class MedicalInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("medical_infor_id")]
        public int MedicalInforId { get; set; }

        [Column("spo2_information")]
        public float Spo2Information { get; set; }

        [Column("heart_rate")]
        public float HeartRate { get; set; }

        [Column("systolic_pressure")]
        public float SystolicPressure { get; set; }

        [Column("diastolic_pressure")]
        public float DiastolicPressure { get; set; }

        [Column("gps")]
        public string GPS { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public StrokeUser StrokeUser { get; set; }
    }
}
