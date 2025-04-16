using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("indicator_summary")]
    public class IndicatorSummary
    {
        [Key]
        [Column("SummaryID")]
        public int SummaryID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("RecordedAt", TypeName = "datetime(6)")]
        public DateTime RecordedAt { get; set; }

        [Column("ClinicalScore", TypeName = "decimal(10,4)")]
        public decimal? ClinicalScore { get; set; }

        [Column("MolecularScore", TypeName = "decimal(10,4)")]
        public decimal? MolecularScore { get; set; }

        [Column("SubclinicalScore", TypeName = "decimal(10,4)")]
        public decimal? SubclinicalScore { get; set; }

        [Column("CombinedScore", TypeName = "decimal(10,4)")]
        public decimal? CombinedScore { get; set; }

        [ForeignKey("UserID")]
        public virtual StrokeUser StrokeUser { get; set; }
    }
}
