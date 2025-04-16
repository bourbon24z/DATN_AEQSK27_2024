using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("molecular_indicator")]
    public class MolecularIndicator
    {
        [Key]
        [Column("MolecularIndicatorID")]
        public int MolecularIndicatorID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("IsActived")]
        public bool IsActived { get; set; } = true;

        [Column("RecordedAt", TypeName = "datetime(6)")]
        public DateTime RecordedAt { get; set; }

        [Column("miR_30e_5p")]
        public bool MiR_30e_5p { get; set; }

        [Column("miR_16_5p")]
        public bool MiR_16_5p { get; set; }

        [Column("miR_140_3p")]
        public bool MiR_140_3p { get; set; }

        [Column("miR_320d")]
        public bool MiR_320d { get; set; }

        [Column("miR_320p")]
        public bool MiR_320p { get; set; }

        [Column("miR_20a_5p")]
        public bool MiR_20a_5p { get; set; }

        [Column("miR_26b_5p")]
        public bool MiR_26b_5p { get; set; }

        [Column("miR_19b_5p")]
        public bool MiR_19b_5p { get; set; }

        [Column("miR_874_5p")]
        public bool MiR_874_5p { get; set; }

        [Column("miR_451a")]
        public bool MiR_451a { get; set; }

        [Column("ReportCount")]
        public int ReportCount { get; set; } = 0;

        [ForeignKey("UserID")]
        public virtual StrokeUser StrokeUser { get; set; }
    }
}
