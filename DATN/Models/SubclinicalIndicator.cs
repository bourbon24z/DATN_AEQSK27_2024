using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("subclinical_indicator")]
    public class SubclinicalIndicator
    {
        [Key]
        [Column("SubclinicalIndicatorID")]
        public int SubclinicalIndicatorID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("IsActived")]
        public bool IsActived { get; set; } = true;

        [Column("RecordedAt", TypeName = "datetime(6)")]
        public DateTime RecordedAt { get; set; }

        [Column("S100B")]
        public bool S100B { get; set; }

        [Column("MMP9")]
        public bool MMP9 { get; set; }

        [Column("GFAP")]
        public bool GFAP { get; set; }

        [Column("RBP4")]
        public bool RBP4 { get; set; }

        [Column("NT_proBNP")]
        public bool NT_proBNP { get; set; }

        [Column("sRAGE")]
        public bool sRAGE { get; set; }

        [Column("D_dimer")]
        public bool D_dimer { get; set; }

        [Column("Lipids")]
        public bool Lipids { get; set; }

        [Column("Protein")]
        public bool Protein { get; set; }

        [Column("vonWillebrand")]
        public bool VonWillebrand { get; set; }

        [Column("ReportCount")]
        public int ReportCount { get; set; } = 0;

        [ForeignKey("UserID")]
        public virtual StrokeUser StrokeUser { get; set; }
    }
}
