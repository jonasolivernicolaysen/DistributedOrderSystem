using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountBalanceToUserClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Lastname",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.AddColumn<double>(
                name: "AccountBalance",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBalance",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "Lastname");
        }
    }
}
