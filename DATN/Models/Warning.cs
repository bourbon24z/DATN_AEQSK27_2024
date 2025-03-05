using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class Warning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("warning_id")]
        public int WarningId { get; set; }
        [Column("warning_patient_id")]
        public int WarningPatientId { get; set; }
        public int Type { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Status { get; set; }
        public Patient Patient { get; set; }
    }
}
