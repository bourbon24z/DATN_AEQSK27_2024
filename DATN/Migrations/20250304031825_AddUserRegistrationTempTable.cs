using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRegistrationTempTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseHistories_Patients_PatientId",
                table: "CaseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalInformations_Patients_PatientId",
                table: "MedicalInformations");

            migrationBuilder.DropForeignKey(
                name: "FK_StrokeUsers_Patients_PatientId",
                table: "StrokeUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warnings_Patients_PatientId",
                table: "Warnings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warnings",
                table: "Warnings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StrokeUsers",
                table: "StrokeUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patients",
                table: "Patients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalInformations",
                table: "MedicalInformations");

            migrationBuilder.DropIndex(
                name: "IX_MedicalInformations_PatientId",
                table: "MedicalInformations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseHistories",
                table: "CaseHistories");

            migrationBuilder.DropIndex(
                name: "IX_CaseHistories_PatientId",
                table: "CaseHistories");

            migrationBuilder.RenameTable(
                name: "Warnings",
                newName: "Warning");

            migrationBuilder.RenameTable(
                name: "StrokeUsers",
                newName: "StrokeUser");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "Patient");

            migrationBuilder.RenameTable(
                name: "MedicalInformations",
                newName: "Medical_Information");

            migrationBuilder.RenameTable(
                name: "CaseHistories",
                newName: "CaseHistory");

            migrationBuilder.RenameColumn(
                name: "WarningId",
                table: "Warning",
                newName: "warning_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Warning",
                newName: "warning_patient_id");

            migrationBuilder.RenameIndex(
                name: "IX_Warnings_PatientId",
                table: "Warning",
                newName: "IX_Warning_warning_patient_id");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "StrokeUser",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "StrokeUser",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "StrokeUser",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "StrokeUser",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "StrokeUser",
                newName: "user_patient_id");

            migrationBuilder.RenameIndex(
                name: "IX_StrokeUsers_PatientId",
                table: "StrokeUser",
                newName: "IX_StrokeUser_user_patient_id");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Patient",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Patient",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Patient",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PatientName",
                table: "Patient",
                newName: "patient_name");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Patient",
                newName: "date_of_birth");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Patient",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "MedicalInformationId",
                table: "Medical_Information",
                newName: "medical_infor_id");

            migrationBuilder.RenameColumn(
                name: "CaseHistoryId",
                table: "CaseHistory",
                newName: "case_history_id");

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "StrokeUser",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Patient",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "mi_patient_id",
                table: "Medical_Information",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ch_patient_id",
                table: "CaseHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warning",
                table: "Warning",
                column: "warning_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StrokeUser",
                table: "StrokeUser",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patient",
                table: "Patient",
                column: "patient_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medical_Information",
                table: "Medical_Information",
                column: "medical_infor_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseHistory",
                table: "CaseHistory",
                column: "case_history_id");

            migrationBuilder.CreateTable(
                name: "UserRegistrationTemps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Otp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegistrationTemps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Information_mi_patient_id",
                table: "Medical_Information",
                column: "mi_patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistory_ch_patient_id",
                table: "CaseHistory",
                column: "ch_patient_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseHistory_Patient_ChPatientId",
                table: "CaseHistory",
                column: "ch_patient_id",
                principalTable: "Patient",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalInformation_Patient_MiPatientId",
                table: "Medical_Information",
                column: "mi_patient_id",
                principalTable: "Patient",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrokeUser_Patient_user_patient_id",
                table: "StrokeUser",
                column: "user_patient_id",
                principalTable: "Patient",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Warning_Patient_WarningPatientId",
                table: "Warning",
                column: "warning_patient_id",
                principalTable: "Patient",
                principalColumn: "patient_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseHistory_Patient_ChPatientId",
                table: "CaseHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalInformation_Patient_MiPatientId",
                table: "Medical_Information");

            migrationBuilder.DropForeignKey(
                name: "FK_StrokeUser_Patient_user_patient_id",
                table: "StrokeUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Warning_Patient_WarningPatientId",
                table: "Warning");

            migrationBuilder.DropTable(
                name: "UserRegistrationTemps");

            migrationBuilder.DropTable(
                name: "UserVerifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warning",
                table: "Warning");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StrokeUser",
                table: "StrokeUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patient",
                table: "Patient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Medical_Information",
                table: "Medical_Information");

            migrationBuilder.DropIndex(
                name: "IX_Medical_Information_mi_patient_id",
                table: "Medical_Information");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseHistory",
                table: "CaseHistory");

            migrationBuilder.DropIndex(
                name: "IX_CaseHistory_ch_patient_id",
                table: "CaseHistory");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "mi_patient_id",
                table: "Medical_Information");

            migrationBuilder.DropColumn(
                name: "ch_patient_id",
                table: "CaseHistory");

            migrationBuilder.RenameTable(
                name: "Warning",
                newName: "Warnings");

            migrationBuilder.RenameTable(
                name: "StrokeUser",
                newName: "StrokeUsers");

            migrationBuilder.RenameTable(
                name: "Patient",
                newName: "Patients");

            migrationBuilder.RenameTable(
                name: "Medical_Information",
                newName: "MedicalInformations");

            migrationBuilder.RenameTable(
                name: "CaseHistory",
                newName: "CaseHistories");

            migrationBuilder.RenameColumn(
                name: "warning_id",
                table: "Warnings",
                newName: "WarningId");

            migrationBuilder.RenameColumn(
                name: "warning_patient_id",
                table: "Warnings",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Warning_warning_patient_id",
                table: "Warnings",
                newName: "IX_Warnings_PatientId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "StrokeUsers",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "StrokeUsers",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "StrokeUsers",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "StrokeUsers",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "user_patient_id",
                table: "StrokeUsers",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_StrokeUser_user_patient_id",
                table: "StrokeUsers",
                newName: "IX_StrokeUsers_PatientId");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Patients",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "gender",
                table: "Patients",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Patients",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "patient_name",
                table: "Patients",
                newName: "PatientName");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "Patients",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "Patients",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "medical_infor_id",
                table: "MedicalInformations",
                newName: "MedicalInformationId");

            migrationBuilder.RenameColumn(
                name: "case_history_id",
                table: "CaseHistories",
                newName: "CaseHistoryId");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "StrokeUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warnings",
                table: "Warnings",
                column: "WarningId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StrokeUsers",
                table: "StrokeUsers",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                table: "Patients",
                column: "PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalInformations",
                table: "MedicalInformations",
                column: "MedicalInformationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseHistories",
                table: "CaseHistories",
                column: "CaseHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalInformations_PatientId",
                table: "MedicalInformations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistories_PatientId",
                table: "CaseHistories",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseHistories_Patients_PatientId",
                table: "CaseHistories",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalInformations_Patients_PatientId",
                table: "MedicalInformations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrokeUsers_Patients_PatientId",
                table: "StrokeUsers",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Warnings_Patients_PatientId",
                table: "Warnings",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
