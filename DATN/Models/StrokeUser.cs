﻿using DATN.Verification;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class StrokeUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("patient_name")]
        public string PatientName { get; set; }

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("gender")]
        public bool Gender { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; }
        public virtual ICollection<Gps> Gps { get; set; } = new List<Gps>();
        public ICollection<InvitationCode> InvitationCodes { get; set; }

        public ICollection<Relationship> Relationships { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<Device> Devices { get; set; } = new List<Device>();

        public ICollection<Warning> Warnings { get; set; } = new List<Warning>();

        public ICollection<CaseHistory> CaseHistories { get; set; } = new List<CaseHistory>();

        public ICollection<UserVerification> UserVerifications { get; set; } = new List<UserVerification>();

        public ICollection <MedicalHistoryValue> MedicalHistoryValues { get; set; } = new List<MedicalHistoryValue>();


    }
}

