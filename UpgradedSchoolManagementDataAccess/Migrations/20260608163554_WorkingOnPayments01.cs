using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpgradedSchoolManagementDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class WorkingOnPayments01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecordedBy",
                table: "StudentPayments",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentPayments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordedBy",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentPayments");
        }
    }
}
