using Microsoft.EntityFrameworkCore;
using DATN.Models;
using DATN.Verification;

namespace DATN.Data
{
    public class StrokeDbContext : DbContext
    {
        public StrokeDbContext(DbContextOptions<StrokeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<StrokeUser> StrokeUsers { get; set; }
        public DbSet<CaseHistory> CaseHistories { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<MedicalInformation> MedicalInformations { get; set; }

        public DbSet<UserVerification> UserVerifications { get; set; }

        public DbSet<UserRegistrationTemp> UserRegistrationTemps { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StrokeUser>().ToTable("StrokeUser");

            modelBuilder.Entity<StrokeUser>()
                .Property(u => u.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<StrokeUser>()
                .Property(u => u.UserPatientId)
                .HasColumnName("user_patient_id");

            modelBuilder.Entity<StrokeUser>()
                .HasOne(u => u.Patient)
                .WithMany()
                .HasForeignKey(u => u.UserPatientId);

            modelBuilder.Entity<Patient>().ToTable("Patient");

            modelBuilder.Entity<Patient>()
                .Property(p => p.PatientId)
                .HasColumnName("patient_id");

            modelBuilder.Entity<CaseHistory>().ToTable("CaseHistory");

            modelBuilder.Entity<CaseHistory>()
                .Property(c => c.CaseHistoryId)
                .HasColumnName("case_history_id");

            modelBuilder.Entity<CaseHistory>()
                .Property(c => c.ChPatientId)
                .HasColumnName("ch_patient_id");

            modelBuilder.Entity<Warning>().ToTable("Warning");

            modelBuilder.Entity<Warning>()
                .Property(w => w.WarningId)
                .HasColumnName("warning_id");

            modelBuilder.Entity<Warning>()
                .Property(w => w.WarningPatientId)
                .HasColumnName("warning_patient_id");

            modelBuilder.Entity<MedicalInformation>().ToTable("Medical_Information");

            modelBuilder.Entity<MedicalInformation>()
                .Property(m => m.MedicalInforId)
                .HasColumnName("medical_infor_id");

            modelBuilder.Entity<MedicalInformation>()
                .Property(m => m.MiPatientId)
                .HasColumnName("mi_patient_id");

            modelBuilder.Entity<CaseHistory>()
                .HasOne(c => c.Patient)
                .WithMany()
                .HasForeignKey(c => c.ChPatientId)
                .HasConstraintName("FK_CaseHistory_Patient_ChPatientId");

            modelBuilder.Entity<Warning>()
                .HasOne(w => w.Patient)
                .WithMany()
                .HasForeignKey(w => w.WarningPatientId)
                .HasConstraintName("FK_Warning_Patient_WarningPatientId");

            modelBuilder.Entity<MedicalInformation>()
                .HasOne(m => m.Patient)
                .WithMany()
                .HasForeignKey(m => m.MiPatientId)
                .HasConstraintName("FK_MedicalInformation_Patient_MiPatientId");
            modelBuilder.Entity<UserRegistrationTemp>().ToTable("UserRegistrationTemp");

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Id)
                .HasColumnName("id");

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Password)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Role)
                .HasColumnName("role")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.Otp)
                .HasColumnName("otp")
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<UserRegistrationTemp>()
                .Property(u => u.OtpExpiry)
                .HasColumnName("otp_expiry")
                .HasColumnType("datetime")
                .IsRequired();

        }
    }
}
