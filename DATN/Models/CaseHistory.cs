using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class CaseHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("case_history_id")]
        public int CaseHistoryId { get; set; }

        [Column("progress_notes")]
        public string ProgressNotes { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("status_of_mr")]
        public string StatusOfMr { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public StrokeUser StrokeUser { get; set; }
    }
}
