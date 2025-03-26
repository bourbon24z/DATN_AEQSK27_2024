﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<MedicalInformation> MedicalInformations { get; set; }
        public DbSet<CaseHistory> CaseHistories { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<UserRegistrationTemp> UserRegistrationTemps { get; set; }
        public DbSet<InvitationCode> InvitationCodes { get; set; }
        public DbSet<Relationship> Relationships { get; set; }


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

           
            modelBuilder.Entity<InvitationCode>(entity =>
            {
                entity.HasKey(i => i.InvitationId); 

                entity.HasOne(ic => ic.InviterUser) 
                      .WithMany(u => u.InvitationCodes)
                      .HasForeignKey(ic => ic.InviterUserId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });


            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.HasKey(r => r.RelationshipId); 

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Relationships)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(r => r.Inviter) 
                      .WithMany() 
                      .HasForeignKey(r => r.InviterId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

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
