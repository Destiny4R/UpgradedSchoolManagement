using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpgradedSchoolManagementDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ResultManager01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appsettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Term = table.Column<int>(type: "int", nullable: true),
                    SchoolClassId = table.Column<int>(type: "int", nullable: true),
                    SubClassId = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    PrincipalName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    PrincipalSignature = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    CashierName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CashierSignature = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appsettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appsettings_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appsettings_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appsettings_SesseionTables_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SesseionTables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appsettings_SubClassTables_SubClassId",
                        column: x => x.SubClassId,
                        principalTable: "SubClassTables",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassTermInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Term = table.Column<int>(type: "int", nullable: false),
                    SchoolClassId = table.Column<int>(type: "int", nullable: false),
                    SubClassId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    NextTermFees = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ClassTeacherName = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicationUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTermInformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTermInformations_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTermInformations_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTermInformations_SesseionTables_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SesseionTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTermInformations_SubClassTables_SubClassId",
                        column: x => x.SubClassId,
                        principalTable: "SubClassTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TermGeneralInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Term = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    DaySchoolOpen = table.Column<int>(type: "int", nullable: false),
                    PrincipalName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NextTermStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NextTermEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermGeneralInformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermGeneralInformations_SesseionTables_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SesseionTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Appsettings_ApplicationUserId",
                table: "Appsettings",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appsettings_SchoolClassId",
                table: "Appsettings",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Appsettings_SessionId",
                table: "Appsettings",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Appsettings_SubClassId",
                table: "Appsettings",
                column: "SubClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTermInformations_ApplicationUserId",
                table: "ClassTermInformations",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTermInformations_SchoolClassId",
                table: "ClassTermInformations",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTermInformations_SessionId",
                table: "ClassTermInformations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTermInformations_SubClassId",
                table: "ClassTermInformations",
                column: "SubClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TermGeneralInformations_SessionId",
                table: "TermGeneralInformations",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appsettings");

            migrationBuilder.DropTable(
                name: "ClassTermInformations");

            migrationBuilder.DropTable(
                name: "TermGeneralInformations");
        }
    }
}
