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
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<CaseHistory> CaseHistories { get; set; }
        public DbSet<UserMedicalData> UserMedicalDatas { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<UserRegistrationTemp> UserRegistrationTemps { get; set; }
        public DbSet<InvitationCode> InvitationCodes { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }


        // Các DbSet cho bảng chẩn đoán đột quỵ
        public DbSet<DoctorEvaluation> DoctorEvaluations { get; set; }
        public DbSet<MedicalImage> MedicalImages { get; set; }
        public DbSet<IndicatorSummary> IndicatorSummaries { get; set; }
        public DbSet<ClinicalIndicator> ClinicalIndicators { get; set; }
        public DbSet<MolecularIndicator> MolecularIndicators { get; set; }
        public DbSet<SubclinicalIndicator> SubclinicalIndicators { get; set; }

        // Các DbSet cho bảng EAV dạng checklist
        public DbSet<MedicalHistoryAttribute> MedicalHistoryAttributes { get; set; }
        public DbSet<MedicalHistoryValue> MedicalHistoryValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. StrokeUser
            modelBuilder.Entity<StrokeUser>(entity =>
            {
                entity.ToTable("stroke_user");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId).HasColumnName("user_id");
                entity.Property(u => u.Username).HasColumnName("username");
                entity.Property(u => u.Password).HasColumnName("password");
                entity.Property(u => u.PatientName).HasColumnName("patient_name");
                entity.Property(u => u.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(u => u.Gender).HasColumnName("gender");
                entity.Property(u => u.Phone).HasColumnName("phone");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.CreatedAt).HasColumnName("created_at");
                entity.Property(u => u.IsVerified)
                      .HasColumnName("is_verified")
                      .HasDefaultValue(false);
                entity.Property(u => u.Gps)
                      .HasColumnName("gps")
                      .HasMaxLength(255)
                      .IsRequired(false);
            });

            // 2. Warning
            modelBuilder.Entity<Warning>(entity =>
            {
                entity.ToTable("warning");
                entity.HasKey(w => w.WarningId);
                entity.Property(w => w.WarningId).HasColumnName("warning_id");
                entity.Property(w => w.Description).HasColumnName("description");
                entity.Property(w => w.CreatedAt).HasColumnName("created_at");
                entity.Property(w => w.IsActive).HasColumnName("is_active");
                entity.Property(w => w.UserId).HasColumnName("user_id");

                entity.HasOne(w => w.StrokeUser)
                      .WithMany(u => u.Warnings)
                      .HasForeignKey(w => w.UserId)
                      .HasConstraintName("FK_Warning_StrokeUser_UserId");
            });

            // 3. CaseHistory
            modelBuilder.Entity<CaseHistory>(entity =>
            {
                entity.ToTable("case_history");
                entity.HasKey(c => c.CaseHistoryId);
                entity.Property(c => c.CaseHistoryId).HasColumnName("case_history_id");
                entity.Property(c => c.ProgressNotes).HasColumnName("progress_notes");
                entity.Property(c => c.Time).HasColumnName("time");
                entity.Property(c => c.StatusOfMr).HasColumnName("status_of_mr");
                entity.Property(c => c.UserId).HasColumnName("user_id");

                entity.HasOne(c => c.StrokeUser)
                      .WithMany(u => u.CaseHistories)
                      .HasForeignKey(c => c.UserId)
                      .HasConstraintName("FK_CaseHistory_StrokeUser_UserId");
            });

            // 4. UserRegistrationTemp
            modelBuilder.Entity<UserRegistrationTemp>(entity =>
            {
                entity.ToTable("user_registration_temp");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Username)
                      .HasColumnName("username")
                      .HasMaxLength(50)
                      .IsRequired();
                entity.Property(u => u.Password)
                      .HasColumnName("password")
                      .HasMaxLength(255)
                      .IsRequired();
                entity.Property(u => u.Email)
                      .HasColumnName("email")
                      .HasMaxLength(50)
                      .IsRequired();
                entity.Property(u => u.Otp)
                      .HasColumnName("otp")
                      .HasMaxLength(10)
                      .IsRequired();
                entity.Property(u => u.OtpExpiry)
                      .HasColumnName("otp_expiry")
                      .HasColumnType("datetime")
                      .IsRequired();
            });

            // 5. InvitationCode
            modelBuilder.Entity<InvitationCode>(entity =>
            {
                entity.ToTable("invitation_codes");
                entity.HasKey(i => i.InvitationId);
                entity.Property(i => i.InvitationId).HasColumnName("invitation_id");
                entity.Property(i => i.Code).HasColumnName("code");
                entity.Property(i => i.InviterUserId).HasColumnName("inviter_user_id");
                entity.Property(i => i.Status).HasColumnName("status");
                entity.Property(i => i.CreatedAt).HasColumnName("created_at");
                entity.Property(i => i.ExpiresAt).HasColumnName("expires_at");

                entity.HasOne(i => i.InviterUser)
                      .WithMany(u => u.InvitationCodes)
                      .HasForeignKey(i => i.InviterUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Relationship
            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.ToTable("relationships");
                entity.HasKey(r => r.RelationshipId);
                entity.Property(r => r.RelationshipId).HasColumnName("relationship_id");
                entity.Property(r => r.UserId).HasColumnName("user_id");
                entity.Property(r => r.InviterId).HasColumnName("inviter_id");
                entity.Property(r => r.RelationshipType).HasColumnName("relationship_type");
                entity.Property(r => r.CreatedAt).HasColumnName("created_at");

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Relationships)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Inviter)
                      .WithMany() 

                      .HasForeignKey(r => r.InviterId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // 7. Device
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("device");
                entity.HasKey(d => d.DeviceId);
                entity.Property(d => d.DeviceId).HasColumnName("device_id");
                entity.Property(d => d.DeviceName).HasColumnName("device_name");
                entity.Property(d => d.DeviceType).HasColumnName("device_type");
                entity.Property(d => d.Series)
                          .HasColumnName("series")
                          .HasMaxLength(50);
            });

            // 8. UserMedicalData
            modelBuilder.Entity<UserMedicalData>(entity =>
            {
                entity.ToTable("user_medical_data");
                entity.HasKey(umd => umd.UserMedicalDataId);
                entity.Property(umd => umd.UserMedicalDataId).HasColumnName("user_medical_data_id");
                entity.Property(umd => umd.UserId).HasColumnName("user_id");
                entity.Property(umd => umd.DeviceId).HasColumnName("device_id");
                entity.Property(umd => umd.SystolicPressure).HasColumnName("systolic_pressure");
                entity.Property(umd => umd.DiastolicPressure).HasColumnName("diastolic_pressure");
                entity.Property(umd => umd.Temperature).HasColumnName("temperature");
                entity.Property(umd => umd.BloodPh).HasColumnName("blood_ph");
                entity.Property(umd => umd.RecordedAt).HasColumnName("recorded_at");
                entity.Property(umd => umd.Spo2Information).HasColumnName("spo2_information");
                entity.Property(umd => umd.HeartRate).HasColumnName("heart_rate");
                entity.Property(umd => umd.CreatedAt).HasColumnName("created_at");
                entity.Property(umd => umd.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(umd => umd.User)
                      .WithMany(u => u.UserMedicalDatas)
                      .HasForeignKey(umd => umd.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(umd => umd.Device)
                      .WithMany(d => d.UserMedicalDatas)
                      .HasForeignKey(umd => umd.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 9. UserRole
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(ur => ur.UserRoleId);
                entity.Property(ur => ur.UserRoleId).HasColumnName("user_role_id");
                entity.Property(ur => ur.UserId).HasColumnName("user_id");
                entity.Property(ur => ur.RoleId).HasColumnName("role_id");
                entity.Property(ur => ur.CreatedAt).HasColumnName("created_at");
                entity.Property(ur => ur.IsActive).HasColumnName("is_active");

                entity.HasOne(ur => ur.StrokeUser)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 10. Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleId).HasColumnName("role_id");
                entity.Property(r => r.RoleName)
                      .HasColumnName("role_name")
                      .HasMaxLength(50)
                      .IsRequired();
            });

            // 11. UserVerification
            modelBuilder.Entity<UserVerification>(entity =>
            {
                entity.ToTable("user_verifications");
                entity.HasKey(uv => uv.Id);
                entity.Property(uv => uv.Id).HasColumnName("id");
                entity.Property(uv => uv.UserId).HasColumnName("user_id");
                entity.Property(uv => uv.Email).HasColumnName("email");
                entity.Property(uv => uv.VerificationCode).HasColumnName("verification_code");
                entity.Property(uv => uv.OtpExpiry).HasColumnName("otp_expiry");
                entity.Property(uv => uv.IsVerified).HasColumnName("is_verified");

                entity.HasOne<StrokeUser>()
                      .WithMany()
                      .HasForeignKey(uv => uv.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_UserVerifications_StrokeUser");
            });
                modelBuilder.Entity<MedicalHistoryValue>()
               .HasKey(mhv => new { mhv.UserId, mhv.AttributeId });

               
                modelBuilder.Entity<MedicalHistoryValue>()
                    .HasOne(mhv => mhv.StrokeUser)
                    .WithMany(su => su.MedicalHistoryValues)
                    .HasForeignKey(mhv => mhv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<MedicalHistoryValue>()
                    .HasOne(mhv => mhv.MedicalHistoryAttribute)
                    .WithMany()
                    .HasForeignKey(mhv => mhv.AttributeId)
                    .OnDelete(DeleteBehavior.Cascade);
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
