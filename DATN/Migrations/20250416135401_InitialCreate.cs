using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    device_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    series = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device", x => x.device_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medicalhistoryattributes",
                columns: table => new
                {
                    ValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AttributeName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unit = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicalhistoryattributes", x => x.ValueId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    role_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stroke_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    patient_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_of_birth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    gender = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_verified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    gps = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stroke_user", x => x.user_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_registration_temp",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    otp = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    otp_expiry = table.Column<DateTime>(type: "datetime", nullable: false),
                    patient_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_of_birth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    gender = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_registration_temp", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "case_history",
                columns: table => new
                {
                    case_history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    progress_notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status_of_mr = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_history", x => x.case_history_id);
                    table.ForeignKey(
                        name: "FK_CaseHistory_StrokeUser_UserId",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clinical_indicator",
                columns: table => new
                {
                    ClinicalIndicatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DauDau = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TeMatChi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ChongMat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    KhoNoi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MatTriNhoTamThoi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LuLan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GiamThiLuc = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MatThangCan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BuonNon = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    KhoNuot = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReportCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_indicator", x => x.ClinicalIndicatorID);
                    table.ForeignKey(
                        name: "FK_clinical_indicator_stroke_user_UserID",
                        column: x => x.UserID,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "indicator_summary",
                columns: table => new
                {
                    SummaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClinicalScore = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    MolecularScore = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SubclinicalScore = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    CombinedScore = table.Column<decimal>(type: "decimal(10,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_summary", x => x.SummaryID);
                    table.ForeignKey(
                        name: "FK_indicator_summary_stroke_user_UserID",
                        column: x => x.UserID,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "invitation_codes",
                columns: table => new
                {
                    invitation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    inviter_user_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitation_codes", x => x.invitation_id);
                    table.ForeignKey(
                        name: "FK_invitation_codes_stroke_user_inviter_user_id",
                        column: x => x.inviter_user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medicalhistoryvalues",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicalhistoryvalues", x => new { x.UserId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_medicalhistoryvalues_medicalhistoryattributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "medicalhistoryattributes",
                        principalColumn: "ValueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_medicalhistoryvalues_stroke_user_UserId",
                        column: x => x.UserId,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "molecular_indicator",
                columns: table => new
                {
                    MolecularIndicatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    miR_30e_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_16_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_140_3p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_320d = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_320p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_20a_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_26b_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_19b_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_874_5p = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    miR_451a = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReportCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_molecular_indicator", x => x.MolecularIndicatorID);
                    table.ForeignKey(
                        name: "FK_molecular_indicator_stroke_user_UserID",
                        column: x => x.UserID,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "relationships",
                columns: table => new
                {
                    relationship_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    inviter_id = table.Column<int>(type: "int", nullable: false),
                    relationship_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationships", x => x.relationship_id);
                    table.ForeignKey(
                        name: "FK_relationships_stroke_user_inviter_id",
                        column: x => x.inviter_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_relationships_stroke_user_user_id",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subclinical_indicator",
                columns: table => new
                {
                    SubclinicalIndicatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    S100B = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MMP9 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GFAP = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RBP4 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NT_proBNP = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sRAGE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    D_dimer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Lipids = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Protein = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    vonWillebrand = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReportCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subclinical_indicator", x => x.SubclinicalIndicatorID);
                    table.ForeignKey(
                        name: "FK_subclinical_indicator_stroke_user_UserID",
                        column: x => x.UserID,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_medical_data",
                columns: table => new
                {
                    user_medical_data_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: true),
                    systolic_pressure = table.Column<float>(type: "float", nullable: true),
                    diastolic_pressure = table.Column<float>(type: "float", nullable: true),
                    temperature = table.Column<float>(type: "float", nullable: true),
                    blood_ph = table.Column<float>(type: "float", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    spo2_information = table.Column<float>(type: "float", nullable: true),
                    heart_rate = table.Column<float>(type: "float", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_medical_data", x => x.user_medical_data_id);
                    table.ForeignKey(
                        name: "FK_user_medical_data_device_device_id",
                        column: x => x.device_id,
                        principalTable: "device",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_medical_data_stroke_user_user_id",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.user_role_id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_stroke_user_user_id",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_verifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verification_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    otp_expiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_verified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StrokeUserUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_verifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserVerifications_StrokeUser",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_verifications_stroke_user_StrokeUserUserId",
                        column: x => x.StrokeUserUserId,
                        principalTable: "stroke_user",
                        principalColumn: "user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warning",
                columns: table => new
                {
                    warning_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warning", x => x.warning_id);
                    table.ForeignKey(
                        name: "FK_Warning_StrokeUser_UserId",
                        column: x => x.user_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "doctor_evaluations",
                columns: table => new
                {
                    doctor_evaluation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    case_history_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    evaluation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    evaluation_notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_evaluations", x => x.doctor_evaluation_id);
                    table.ForeignKey(
                        name: "FK_doctor_evaluations_case_history_case_history_id",
                        column: x => x.case_history_id,
                        principalTable: "case_history",
                        principalColumn: "case_history_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doctor_evaluations_stroke_user_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "stroke_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medical_images",
                columns: table => new
                {
                    medical_image_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    case_history_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    captured_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    metadata = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_images", x => x.medical_image_id);
                    table.ForeignKey(
                        name: "FK_medical_images_case_history_case_history_id",
                        column: x => x.case_history_id,
                        principalTable: "case_history",
                        principalColumn: "case_history_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_case_history_user_id",
                table: "case_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_clinical_indicator_UserID",
                table: "clinical_indicator",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_evaluations_case_history_id",
                table: "doctor_evaluations",
                column: "case_history_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_evaluations_doctor_id",
                table: "doctor_evaluations",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_summary_UserID",
                table: "indicator_summary",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_codes_inviter_user_id",
                table: "invitation_codes",
                column: "inviter_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_medical_images_case_history_id",
                table: "medical_images",
                column: "case_history_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicalhistoryvalues_AttributeId",
                table: "medicalhistoryvalues",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_molecular_indicator_UserID",
                table: "molecular_indicator",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_inviter_id",
                table: "relationships",
                column: "inviter_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_user_id",
                table: "relationships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_subclinical_indicator_UserID",
                table: "subclinical_indicator",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_user_medical_data_device_id",
                table: "user_medical_data",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_medical_data_user_id",
                table: "user_medical_data",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_verifications_StrokeUserUserId",
                table: "user_verifications",
                column: "StrokeUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_verifications_user_id",
                table: "user_verifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_warning_user_id",
                table: "warning",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_indicator");

            migrationBuilder.DropTable(
                name: "doctor_evaluations");

            migrationBuilder.DropTable(
                name: "indicator_summary");

            migrationBuilder.DropTable(
                name: "invitation_codes");

            migrationBuilder.DropTable(
                name: "medical_images");

            migrationBuilder.DropTable(
                name: "medicalhistoryvalues");

            migrationBuilder.DropTable(
                name: "molecular_indicator");

            migrationBuilder.DropTable(
                name: "relationships");

            migrationBuilder.DropTable(
                name: "subclinical_indicator");

            migrationBuilder.DropTable(
                name: "user_medical_data");

            migrationBuilder.DropTable(
                name: "user_registration_temp");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_verifications");

            migrationBuilder.DropTable(
                name: "warning");

            migrationBuilder.DropTable(
                name: "case_history");

            migrationBuilder.DropTable(
                name: "medicalhistoryattributes");

            migrationBuilder.DropTable(
                name: "device");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "stroke_user");
        }
    }
}
