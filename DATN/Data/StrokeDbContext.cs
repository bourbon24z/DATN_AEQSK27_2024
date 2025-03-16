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

        public DbSet<StrokeUser> StrokeUsers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<MedicalInformation> MedicalInformations { get; set; }
        public DbSet<CaseHistory> CaseHistories { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<UserRegistrationTemp> UserRegistrationTemps { get; set; }
        public DbSet<ContactRegistrationTemp> ContactRegistrationTemps { get; set; }
        public DbSet<ContactPatient> ContactPatients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StrokeUser>().ToTable("stroke_user");
            modelBuilder.Entity<StrokeUser>()
                .Property(u => u.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<StrokeUser>()
                .Property(u => u.IsVerified)
                .HasColumnName("is_verified")
                .HasDefaultValue(false);

            modelBuilder.Entity<Contact>().ToTable("contact");
            modelBuilder.Entity<Contact>()
                .Property(c => c.ContactId)
                .HasColumnName("contact_id");

            modelBuilder.Entity<ContactPatient>().ToTable("contact_patient");
            modelBuilder.Entity<ContactPatient>()
                .Property(cp => cp.ContactPatientId)
                .HasColumnName("contact_patient_id");

            modelBuilder.Entity<ContactPatient>()
                .HasOne(cp => cp.Contact)
                .WithMany()
                .HasForeignKey(cp => cp.ContactId);

            modelBuilder.Entity<ContactPatient>()
                .HasOne(cp => cp.StrokeUser)
                .WithMany()
                .HasForeignKey(cp => cp.UserId);

            modelBuilder.Entity<Warning>().ToTable("warning");
            modelBuilder.Entity<Warning>()
                .Property(w => w.WarningId)
                .HasColumnName("warning_id");

            modelBuilder.Entity<Warning>()
                .Property(w => w.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Warning>()
                .HasOne(w => w.StrokeUser)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .HasConstraintName("FK_Warning_StrokeUser_UserId");

            modelBuilder.Entity<MedicalInformation>().ToTable("medical_information");
            modelBuilder.Entity<MedicalInformation>()
                .Property(m => m.MedicalInforId)
                .HasColumnName("medical_infor_id");

            modelBuilder.Entity<MedicalInformation>()
                .Property(m => m.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<MedicalInformation>()
                .HasOne(m => m.StrokeUser)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .HasConstraintName("FK_MedicalInformation_StrokeUser_UserId");

            modelBuilder.Entity<CaseHistory>().ToTable("case_history");
            modelBuilder.Entity<CaseHistory>()
                .Property(c => c.CaseHistoryId)
                .HasColumnName("case_history_id");

            modelBuilder.Entity<CaseHistory>()
                .Property(c => c.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<CaseHistory>()
                .HasOne(c => c.StrokeUser)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .HasConstraintName("FK_CaseHistory_StrokeUser_UserId");

            modelBuilder.Entity<UserRegistrationTemp>().ToTable("user_registration_temp");
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

            modelBuilder.Entity<ContactRegistrationTemp>().ToTable("contact_registration_temp");
            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Relationship)
                .HasColumnName("relationship")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Email)
                .HasColumnName("email")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.PatientEmail)
                .HasColumnName("patient_email")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Password)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.Otp)
                .HasColumnName("otp")
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<ContactRegistrationTemp>()
                .Property(c => c.OtpExpiry)
                .HasColumnName("otp_expiry")
                .HasColumnType("datetime")
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "server=localhost;port=3306;database=demo_db_stroke;user=root;password=";
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
            }
        }
    }
}
