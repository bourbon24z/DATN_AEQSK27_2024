using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class UserRegistrationTemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("otp")]
        public string Otp { get; set; }

        [Column("otp_expiry")]
        public DateTime OtpExpiry { get; set; }

        [Column("patient_name")]
        public string PatientName { get; set; }

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("gender")]
        public bool Gender { get; set; }

        [Column("phone")]
        public string Phone { get; set; }
    }
}
