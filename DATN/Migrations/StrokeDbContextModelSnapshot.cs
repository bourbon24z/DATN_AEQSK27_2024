﻿// <auto-generated />
using System;
using DATN.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DATN.Migrations
{
    [DbContext(typeof(StrokeDbContext))]
    partial class StrokeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CaseHistoryId"));

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

                    b.HasIndex("PatientId");

                    b.ToTable("CaseHistories");
                });

            modelBuilder.Entity("DATN.Models.MedicalInformation", b =>
                {
                    b.Property<int>("MedicalInformationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MedicalInformationId"));

                    b.Property<float>("DiastolicPressure")
                        .HasColumnType("real");

                    b.Property<string>("GPS")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("HeartRate")
                        .HasColumnType("real");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<float>("Spo2Information")
                        .HasColumnType("real");

                    b.Property<float>("SystolicPressure")
                        .HasColumnType("real");

                    b.HasKey("MedicalInformationId");

                    b.HasIndex("PatientId");

                    b.ToTable("MedicalInformations");
                });

            modelBuilder.Entity("DATN.Models.Patient", b =>
                {
                    b.Property<int>("PatientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PatientId"));

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Gender")
                        .HasColumnType("bit");

                    b.Property<string>("PatientName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PatientId");

                    b.ToTable("Patients");
                });

            modelBuilder.Entity("DATN.Models.StrokeUser", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.HasIndex("PatientId");

                    b.ToTable("StrokeUsers");
                });

            modelBuilder.Entity("DATN.Models.Warning", b =>
                {
                    b.Property<int>("WarningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WarningId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("WarningId");

                    b.HasIndex("PatientId");

                    b.ToTable("Warnings");
                });

            modelBuilder.Entity("DATN.Models.CaseHistory", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.MedicalInformation", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.StrokeUser", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DATN.Models.Warning", b =>
                {
                    b.HasOne("DATN.Models.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });
#pragma warning restore 612, 618
        }
    }
}
