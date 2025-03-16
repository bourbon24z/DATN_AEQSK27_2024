using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class ContactRegistrationTemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("patient_email")]
        public string PatientEmail { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("otp")]
        public string Otp { get; set; }

        [Column("otp_expiry")]
        public DateTime OtpExpiry { get; set; }
    }
}
