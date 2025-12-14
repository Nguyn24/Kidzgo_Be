using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Placeholders = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Placeholders = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    DefaultTuitionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPriceSession = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardStoreItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CostStars = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardStoreItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyReportJobs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AiPayloadRef = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyReportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyReportJobs_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TuitionPlans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    TuitionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPriceSession = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuitionPlans_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TuitionPlans_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashbookEntries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RelatedType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RelatedId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MainTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssistantTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    SchedulePattern = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Users_AssistantTeacherId",
                        column: x => x.AssistantTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Users_MainTeacherId",
                        column: x => x.MainTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: true),
                    AllowanceFixed = table.Column<decimal>(type: "numeric", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlanTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SessionIndex = table.Column<int>(type: "integer", nullable: false),
                    StructureJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplates_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PinHash = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetGroup = table.Column<string>(type: "jsonb", nullable: true),
                    MissionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RewardStars = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Classes_TargetClassId",
                        column: x => x.TargetClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedAssistantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ParticipationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActualDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualAssistantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Classrooms_ActualRoomId",
                        column: x => x.ActualRoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Classrooms_PlannedRoomId",
                        column: x => x.PlannedRoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_ActualAssistantId",
                        column: x => x.ActualAssistantId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_ActualTeacherId",
                        column: x => x.ActualTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_PlannedAssistantId",
                        column: x => x.PlannedAssistantId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_PlannedTeacherId",
                        column: x => x.PlannedTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAttendances",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftHours = table.Column<decimal>(type: "numeric", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "public",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollLines",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollLines_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollLines_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPayments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashbookEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_CashbookEntries_CashbookEntryId",
                        column: x => x.CashbookEntryId,
                        principalSchema: "public",
                        principalTable: "CashbookEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataBefore = table.Column<string>(type: "jsonb", nullable: true),
                    DataAfter = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Profiles_ActorProfileId",
                        column: x => x.ActorProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassEnrollments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PayosPaymentLink = table.Column<string>(type: "text", nullable: true),
                    PayosQr = table.Column<string>(type: "text", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_IssuedBy",
                        column: x => x.IssuedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ZaloId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BranchPreference = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramInterest = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OwnerStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstResponseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TouchCount = table.Column<int>(type: "integer", nullable: false),
                    NextActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConvertedStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Branches_BranchPreference",
                        column: x => x.BranchPreference,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Profiles_ConvertedStudentProfileId",
                        column: x => x.ConvertedStudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_OwnerStaffId",
                        column: x => x.OwnerStaffId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    NoticeHours = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    MonthTag = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Deeplink = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Profiles_RecipientProfileId",
                        column: x => x.RecipientProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentStudentLinks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStudentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentStudentLinks_Profiles_ParentProfileId",
                        column: x => x.ParentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentStudentLinks_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardRedemptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HandledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    HandledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_RewardStoreItems_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "RewardStoreItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Users_HandledBy",
                        column: x => x.HandledBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StarTransactions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarTransactions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StarTransactions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentLevels",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentXp = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLevels_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentMonthlyReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    DraftContent = table.Column<string>(type: "jsonb", nullable: true),
                    FinalContent = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AiVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMonthlyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedByProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Profiles_OpenedByProfileId",
                        column: x => x.OpenedByProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Users_GradedBy",
                        column: x => x.GradedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProgressValue = table.Column<decimal>(type: "numeric", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Missions_MissionId",
                        column: x => x.MissionId,
                        principalSchema: "public",
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AbsenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MarkedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    MarkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Users_MarkedBy",
                        column: x => x.MarkedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkAssignments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Book = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Pages = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Skills = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubmissionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: true),
                    RewardStars = table.Column<int>(type: "integer", nullable: true),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Missions_MissionId",
                        column: x => x.MissionId,
                        principalSchema: "public",
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedContent = table.Column<string>(type: "jsonb", nullable: true),
                    ActualContent = table.Column<string>(type: "jsonb", nullable: true),
                    ActualHomework = table.Column<string>(type: "text", nullable: true),
                    TeacherNotes = table.Column<string>(type: "text", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlans_LessonPlanTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MakeupCredits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedReason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Sessions_SourceSessionId",
                        column: x => x.SourceSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Sessions_UsedSessionId",
                        column: x => x.UsedSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionRoles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PayableUnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    PayableAllowance = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionRoles_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionRoles_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SessionIds = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    NextActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlacementTests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Room = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MeetingLink = table.Column<string>(type: "text", nullable: true),
                    InvigilatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ListeningScore = table.Column<decimal>(type: "numeric", nullable: true),
                    SpeakingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ReadingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    WritingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    LevelRecommendation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProgramRecommendation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Users_InvigilatorUserId",
                        column: x => x.InvigilatorUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportComments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportComments_StudentMonthlyReports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportComments_Users_CommenterId",
                        column: x => x.CommenterId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommenterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommenterProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Profiles_CommenterProfileId",
                        column: x => x.CommenterProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "public",
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketComments_Users_CommenterUserId",
                        column: x => x.CommenterUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkStudents",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AiFeedback = table.Column<string>(type: "jsonb", nullable: true),
                    Attachments = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_HomeworkAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "public",
                        principalTable: "HomeworkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MakeupAllocations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MakeupCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_MakeupCredits_MakeupCreditId",
                        column: x => x.MakeupCreditId,
                        principalSchema: "public",
                        principalTable: "MakeupCredits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_Sessions_TargetSessionId",
                        column: x => x.TargetSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "attendance_unique",
                schema: "public",
                table: "Attendances",
                columns: new[] { "SessionId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedBy",
                schema: "public",
                table: "Attendances",
                column: "MarkedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentProfileId",
                schema: "public",
                table: "Attendances",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorProfileId",
                schema: "public",
                table: "AuditLogs",
                column: "ActorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                schema: "public",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "public",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_BranchId",
                schema: "public",
                table: "CashbookEntries",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_CreatedBy",
                schema: "public",
                table: "CashbookEntries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_ClassId",
                schema: "public",
                table: "ClassEnrollments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_StudentProfileId",
                schema: "public",
                table: "ClassEnrollments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_TuitionPlanId",
                schema: "public",
                table: "ClassEnrollments",
                column: "TuitionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AssistantTeacherId",
                schema: "public",
                table: "Classes",
                column: "AssistantTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_BranchId",
                schema: "public",
                table: "Classes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Code",
                schema: "public",
                table: "Classes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_MainTeacherId",
                schema: "public",
                table: "Classes",
                column: "MainTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ProgramId",
                schema: "public",
                table: "Classes",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_BranchId",
                schema: "public",
                table: "Classrooms",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BranchId",
                schema: "public",
                table: "Contracts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_StaffUserId",
                schema: "public",
                table: "Contracts",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Code",
                schema: "public",
                table: "EmailTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                schema: "public",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_GradedBy",
                schema: "public",
                table: "ExamResults",
                column: "GradedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentProfileId",
                schema: "public",
                table: "ExamResults",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassId",
                schema: "public",
                table: "Exams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CreatedBy",
                schema: "public",
                table: "Exams",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_ClassId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_CreatedBy",
                schema: "public",
                table: "HomeworkAssignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_MissionId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_SessionId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "homework_student_unique",
                schema: "public",
                table: "HomeworkStudents",
                columns: new[] { "AssignmentId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkStudents_StudentProfileId",
                schema: "public",
                table: "HomeworkStudents",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                schema: "public",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BranchId",
                schema: "public",
                table: "Invoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClassId",
                schema: "public",
                table: "Invoices",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssuedBy",
                schema: "public",
                table: "Invoices",
                column: "IssuedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StudentProfileId",
                schema: "public",
                table: "Invoices",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_CreatedBy",
                schema: "public",
                table: "LeadActivities",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_LeadId",
                schema: "public",
                table: "LeadActivities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_BranchPreference",
                schema: "public",
                table: "Leads",
                column: "BranchPreference");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedStudentProfileId",
                schema: "public",
                table: "Leads",
                column: "ConvertedStudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerStaffId",
                schema: "public",
                table: "Leads",
                column: "OwnerStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedBy",
                schema: "public",
                table: "LeaveRequests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ClassId",
                schema: "public",
                table: "LeaveRequests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_StudentProfileId",
                schema: "public",
                table: "LeaveRequests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_SubmittedBy",
                schema: "public",
                table: "LessonPlans",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_TemplateId",
                schema: "public",
                table: "LessonPlans",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "session_unique",
                schema: "public",
                table: "LessonPlans",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_CreatedBy",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_AssignedBy",
                schema: "public",
                table: "MakeupAllocations",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_MakeupCreditId",
                schema: "public",
                table: "MakeupAllocations",
                column: "MakeupCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_TargetSessionId",
                schema: "public",
                table: "MakeupAllocations",
                column: "TargetSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_SourceSessionId",
                schema: "public",
                table: "MakeupCredits",
                column: "SourceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_StudentProfileId",
                schema: "public",
                table: "MakeupCredits",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_UsedSessionId",
                schema: "public",
                table: "MakeupCredits",
                column: "UsedSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_BranchId",
                schema: "public",
                table: "MediaAssets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_ClassId",
                schema: "public",
                table: "MediaAssets",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_StudentProfileId",
                schema: "public",
                table: "MediaAssets",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploaderId",
                schema: "public",
                table: "MediaAssets",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionProgresses_StudentProfileId",
                schema: "public",
                table: "MissionProgresses",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionProgresses_VerifiedBy",
                schema: "public",
                table: "MissionProgresses",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "mission_progress_unique",
                schema: "public",
                table: "MissionProgresses",
                columns: new[] { "MissionId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CreatedBy",
                schema: "public",
                table: "Missions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_TargetClassId",
                schema: "public",
                table: "Missions",
                column: "TargetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportJobs_BranchId",
                schema: "public",
                table: "MonthlyReportJobs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientProfileId",
                schema: "public",
                table: "Notifications",
                column: "RecipientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                schema: "public",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Code",
                schema: "public",
                table: "NotificationTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudentLinks_ParentProfileId",
                schema: "public",
                table: "ParentStudentLinks",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudentLinks_StudentProfileId",
                schema: "public",
                table: "ParentStudentLinks",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ConfirmedBy",
                schema: "public",
                table: "Payments",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "public",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollLines_PayrollRunId",
                schema: "public",
                table: "PayrollLines",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollLines_StaffUserId",
                schema: "public",
                table: "PayrollLines",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_CashbookEntryId",
                schema: "public",
                table: "PayrollPayments",
                column: "CashbookEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_PayrollRunId",
                schema: "public",
                table: "PayrollPayments",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_StaffUserId",
                schema: "public",
                table: "PayrollPayments",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_ApprovedBy",
                schema: "public",
                table: "PayrollRuns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_BranchId",
                schema: "public",
                table: "PayrollRuns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_ClassId",
                schema: "public",
                table: "PlacementTests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_InvigilatorUserId",
                schema: "public",
                table: "PlacementTests",
                column: "InvigilatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_LeadId",
                schema: "public",
                table: "PlacementTests",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_StudentProfileId",
                schema: "public",
                table: "PlacementTests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                schema: "public",
                table: "Profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "public",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "public",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_CommenterId",
                schema: "public",
                table: "ReportComments",
                column: "CommenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_ReportId",
                schema: "public",
                table: "ReportComments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_HandledBy",
                schema: "public",
                table: "RewardRedemptions",
                column: "HandledBy");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_ItemId",
                schema: "public",
                table: "RewardRedemptions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_StudentProfileId",
                schema: "public",
                table: "RewardRedemptions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionRoles_SessionId",
                schema: "public",
                table: "SessionRoles",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionRoles_StaffUserId",
                schema: "public",
                table: "SessionRoles",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualAssistantId",
                schema: "public",
                table: "Sessions",
                column: "ActualAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualRoomId",
                schema: "public",
                table: "Sessions",
                column: "ActualRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualTeacherId",
                schema: "public",
                table: "Sessions",
                column: "ActualTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_BranchId",
                schema: "public",
                table: "Sessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ClassId",
                schema: "public",
                table: "Sessions",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedAssistantId",
                schema: "public",
                table: "Sessions",
                column: "PlannedAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedRoomId",
                schema: "public",
                table: "Sessions",
                column: "PlannedRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedTeacherId",
                schema: "public",
                table: "Sessions",
                column: "PlannedTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_ApprovedBy",
                schema: "public",
                table: "ShiftAttendances",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_ContractId",
                schema: "public",
                table: "ShiftAttendances",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_StaffUserId",
                schema: "public",
                table: "ShiftAttendances",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StarTransactions_CreatedBy",
                schema: "public",
                table: "StarTransactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StarTransactions_StudentProfileId",
                schema: "public",
                table: "StarTransactions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLevels_StudentProfileId",
                schema: "public",
                table: "StudentLevels",
                column: "StudentProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_ReviewedBy",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_SubmittedBy",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CommenterProfileId",
                schema: "public",
                table: "TicketComments",
                column: "CommenterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CommenterUserId",
                schema: "public",
                table: "TicketComments",
                column: "CommenterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                schema: "public",
                table: "TicketComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToUserId",
                schema: "public",
                table: "Tickets",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BranchId",
                schema: "public",
                table: "Tickets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClassId",
                schema: "public",
                table: "Tickets",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OpenedByProfileId",
                schema: "public",
                table: "Tickets",
                column: "OpenedByProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OpenedByUserId",
                schema: "public",
                table: "Tickets",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_BranchId",
                schema: "public",
                table: "TuitionPlans",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId",
                schema: "public",
                table: "TuitionPlans",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                schema: "public",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ClassEnrollments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmailTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamResults",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkStudents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InvoiceLines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LeadActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MakeupAllocations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MediaAssets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MissionProgresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MonthlyReportJobs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ParentStudentLinks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollLines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollPayments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PlacementTests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportComments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RewardRedemptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SessionRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ShiftAttendances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StarTransactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentLevels",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketComments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TuitionPlans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Exams",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkAssignments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlanTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MakeupCredits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CashbookEntries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollRuns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Leads",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentMonthlyReports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RewardStoreItems",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Contracts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Missions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Classes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Classrooms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Programs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "public");
        }
    }
}
