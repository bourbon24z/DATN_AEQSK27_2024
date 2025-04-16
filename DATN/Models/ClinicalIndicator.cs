using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("clinical_indicator")]
    public class ClinicalIndicator
    {
        [Key]
        [Column("ClinicalIndicatorID")]
        public int ClinicalIndicatorID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("IsActived")]
        public bool IsActived { get; set; } = true;

        [Column("RecordedAt", TypeName = "datetime(6)")]
        public DateTime RecordedAt { get; set; }

        [Column("DauDau")]
        public bool DauDau { get; set; }

        [Column("TeMatChi")]
        public bool TeMatChi { get; set; }

        [Column("ChongMat")]
        public bool ChongMat { get; set; }

        [Column("KhoNoi")]
        public bool KhoNoi { get; set; }

        [Column("MatTriNhoTamThoi")]
        public bool MatTriNhoTamThoi { get; set; }

        [Column("LuLan")]
        public bool LuLan { get; set; }

        [Column("GiamThiLuc")]
        public bool GiamThiLuc { get; set; }

        [Column("MatThangCan")]
        public bool MatThangCan { get; set; }

        [Column("BuonNon")]
        public bool BuonNon { get; set; }

        [Column("KhoNuot")]
        public bool KhoNuot { get; set; }

        [Column("ReportCount")]
        public int ReportCount { get; set; } = 0;

        [ForeignKey("UserID")]
        public virtual StrokeUser StrokeUser { get; set; }
    }
}
