using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpgradedSchoolManagementDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "PaymentTransactions",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PaymentTransactions");
        }
    }
}
