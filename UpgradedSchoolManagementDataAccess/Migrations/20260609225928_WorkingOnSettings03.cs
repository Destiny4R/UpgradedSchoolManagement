using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpgradedSchoolManagementDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class WorkingOnSettings03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "TermGeneralInformations",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TermGeneralInformations_ApplicationUserId",
                table: "TermGeneralInformations",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TermGeneralInformations_AspNetUsers_ApplicationUserId",
                table: "TermGeneralInformations",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermGeneralInformations_AspNetUsers_ApplicationUserId",
                table: "TermGeneralInformations");

            migrationBuilder.DropIndex(
                name: "IX_TermGeneralInformations_ApplicationUserId",
                table: "TermGeneralInformations");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "TermGeneralInformations");
        }
    }
}
