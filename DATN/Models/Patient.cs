using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("patient_name")]
        public string PatientName { get; set; }

        [Column("date_of_birth")]
        public DateTimeOffset DateOfBirth { get; set; }

        [Column("gender")]
        public bool Gender { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } 
    }
}

