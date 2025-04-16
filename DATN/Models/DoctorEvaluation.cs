using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("doctor_evaluations")]
    public class DoctorEvaluation
    {
        [Key]
        [Column("doctor_evaluation_id")]
        public int DoctorEvaluationId { get; set; }

        [Column("case_history_id")]
        public int CaseHistoryId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("evaluation_date", TypeName = "datetime(6)")]
        public DateTime EvaluationDate { get; set; }

        [Column("evaluation_notes", TypeName = "longtext")]
        public string EvaluationNotes { get; set; }

        [ForeignKey("CaseHistoryId")]
        public virtual CaseHistory CaseHistory { get; set; }

        [ForeignKey("DoctorId")]
        public virtual StrokeUser Doctor { get; set; }
    }
}
