using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedDoubleWithDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Payments");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Payments");

            migrationBuilder.AddColumn<double>(
                name: "TotalAmount",
                table: "Payments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
