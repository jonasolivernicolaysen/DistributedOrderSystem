using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class AddOutBoxMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Processes",
                table: "OutBoxMessages",
                newName: "Processed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Processed",
                table: "OutBoxMessages",
                newName: "Processes");
        }
    }
}
