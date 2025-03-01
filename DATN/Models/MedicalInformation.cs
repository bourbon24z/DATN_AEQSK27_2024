using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class MedicalInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("medical_infor_id")]
        public int MedicalInforId { get; set; }
        public float Spo2Information { get; set; }
        public float HeartRate { get; set; }
        public float SystolicPressure { get; set; }
        public float DiastolicPressure { get; set; }
        public required string GPS { get; set; }
        public int PatientId { get; set; }
        [Column("mi_patient_id")]
        public int MiPatientId { get; set; }
        public required Patient Patient { get; set; }
   }
}
