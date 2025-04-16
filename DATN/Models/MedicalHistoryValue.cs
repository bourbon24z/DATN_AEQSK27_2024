using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    [Table("medicalhistoryvalues")]
    public class MedicalHistoryValue
    {
        [Column("UserId")]
        public int UserId { get; set; }

        [Column("AttributeId")]
        public int AttributeId { get; set; }

        [Column("RecordedAt", TypeName = "datetime(6)")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual StrokeUser StrokeUser { get; set; }

        [ForeignKey("AttributeId")]
        public virtual MedicalHistoryAttribute MedicalHistoryAttribute { get; set; }
    }
}
