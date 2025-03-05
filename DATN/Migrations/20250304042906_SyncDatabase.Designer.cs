﻿// <auto-generated />
using System;
using DATN.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DATN.Migrations
{
    [DbContext(typeof(StrokeDbContext))]
    [Migration("20250304042906_SyncDatabase")]
    partial class SyncDatabase
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DATN.Models.CaseHistory", b =>
                {
                    b.Property<int>("CaseHistoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("case_history_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CaseHistoryId"));

                    b.Property<int>("ChPatientId")
                        .HasColumnType("int")
                        .HasColumnName("ch_patient_id");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<string>("ProgressNotes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusOfMr")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.HasKey("CaseHistoryId");

                    b.HasIndex("ChPatientId");

                    b.ToTable("CaseHistory", (string)null);
                });

            modelBuilder.Entity("DATN.Models.MedicalInformation", b =>
                {
                    b.Property<int>("MedicalInforId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("medical_infor_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MedicalInforId"));

                    b.Property<float>("DiastolicPressure")
                        .HasColumnType("real");

                    b.Property<string>("GPS")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("HeartRate")
                        .HasColumnType("real");

                    b.Property<int>("MiPatientId")
                        .HasColumnType("int")
                        .HasColumnName("mi_patient_id");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<float>("Spo2Information")
                        .HasColumnType("real");

                    b.Property<float>("SystolicPressure")
                        .HasColumnType("real");

                    b.HasKey("MedicalInforId");

                    b.HasIndex("MiPatientId");

                    b.ToTable("Medical_Information", (string)null);
                });

            modelBuilder.Entity("DATN.Models.Patient", b =>
                {
                    b.Property<int>("PatientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("patient_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PatientId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("created_at");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2")
                        .HasColumnName("date_of_birth");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("email");

                    b.Property<bool>("Gender")
                        .HasColumnType("bit")
                        .HasColumnName("gender");

                    b.Property<string>("PatientName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("patient_name");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("phone");

                    b.HasKey("PatientId");

                    b.ToTable("Patient", (string)null);
                });

            modelBuilder.Entity("DATN.Models.StrokeUser", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("password");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("role");

                    b.Property<int>("UserPatientId")
                        .HasColumnType("int")
                        .HasColumnName("user_patient_id");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("username");

                    b.HasKey("UserId");

                    b.HasIndex("UserPatientId");

                    b.ToTable("StrokeUser", (string)null);
                });

            modelBuilder.Entity("DATN.Models.Warning", b =>
                {
                    b.Property<int>("WarningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("warning_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WarningId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("WarningPatientId")
                        .HasColumnType("int")
                        .HasColumnName("warning_patient_id");

                    b.HasKey("WarningId");

                    b.HasIndex("WarningPatientId");

                    b.ToTable("Warning", (string)null);
                });

            modelBuilder.Entity("DATN.Verification.UserRegistrationTemp", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Otp")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OtpExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UserRegistrationTemps");
                });

            modelBuilder.Entity("DATN.Verification.UserVerification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("VerificationCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UserVerifications");
                });

            modelBuilder.Entity("DATN.Models.CaseHistory", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("ChPatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CaseHistory_Patient_ChPatientId");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.MedicalInformation", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("MiPatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_MedicalInformation_Patient_MiPatientId");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.StrokeUser", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("UserPatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.Warning", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("WarningPatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Warning_Patient_WarningPatientId");

                    b.Navigation("Patient");
                });
#pragma warning restore 612, 618
        }
    }
}
