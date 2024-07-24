using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBA_Web_App.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BillPay",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillPay");
        }
    }
}
