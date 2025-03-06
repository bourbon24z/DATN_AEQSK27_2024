using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class CaseHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("case_history_id")]
        public int CaseHistoryId { get; set; }
        public required string ProgressNotes { get; set; }
        public DateTimeOffset Time { get; set; }
        public required string StatusOfMr { get; set; }
       
        [Column("ch_patient_id")]
        public int ChPatientId { get; set; }
        public required Patient Patient { get; set; }

    }
}
