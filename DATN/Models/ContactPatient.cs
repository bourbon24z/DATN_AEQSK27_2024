using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class ContactPatient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("contact_patient_id")]
        public int ContactPatientId { get; set; }

        [Column("contact_id")]
        public int ContactId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("ContactId")]
        public Contact Contact { get; set; }

        [ForeignKey("UserId")]
        public StrokeUser StrokeUser { get; set; }
    }
}
