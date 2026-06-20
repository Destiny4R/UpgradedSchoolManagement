using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpgradedSchoolManagementDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminalResultSkillConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResultSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Domain = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultSkills", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassResultSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SchoolClassId = table.Column<int>(type: "int", nullable: false),
                    ResultSkillId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassResultSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassResultSkills_ResultSkills_ResultSkillId",
                        column: x => x.ResultSkillId,
                        principalTable: "ResultSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassResultSkills_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StudentResultSkillRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TermRegId = table.Column<long>(type: "bigint", nullable: false),
                    ResultSkillId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentResultSkillRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentResultSkillRatings_ResultSkills_ResultSkillId",
                        column: x => x.ResultSkillId,
                        principalTable: "ResultSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentResultSkillRatings_TermRegistrations_TermRegId",
                        column: x => x.TermRegId,
                        principalTable: "TermRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ClassResultSkills_ResultSkillId",
                table: "ClassResultSkills",
                column: "ResultSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassResultSkills_SchoolClassId_ResultSkillId",
                table: "ClassResultSkills",
                columns: new[] { "SchoolClassId", "ResultSkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentResultSkillRatings_ResultSkillId",
                table: "StudentResultSkillRatings",
                column: "ResultSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentResultSkillRatings_TermRegId_ResultSkillId",
                table: "StudentResultSkillRatings",
                columns: new[] { "TermRegId", "ResultSkillId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassResultSkills");

            migrationBuilder.DropTable(
                name: "StudentResultSkillRatings");

            migrationBuilder.DropTable(
                name: "ResultSkills");
        }
    }
}
