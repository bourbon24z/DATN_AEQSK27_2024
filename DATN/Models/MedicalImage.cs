using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("medical_images")]
    public class MedicalImage
    {
        [Key]
        [Column("medical_image_id")]
        public int MedicalImageId { get; set; }

        [Column("case_history_id")]
        public int CaseHistoryId { get; set; }

        [Column("image_url", TypeName = "varchar(255)")]
        public string ImageUrl { get; set; }

        [Column("captured_at", TypeName = "datetime(6)")]
        public DateTime CapturedAt { get; set; }

        [Column("metadata", TypeName = "longtext")]
        public string Metadata { get; set; }

        [ForeignKey("CaseHistoryId")]
        public virtual CaseHistory CaseHistory { get; set; }
    }
}
