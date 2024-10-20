using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class TeamFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the TeamsTeamId column from Employees table
            migrationBuilder.DropColumn(
                name: "TeamsTeamId",
                table: "Employees");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the TeamsTeamId column in case of rollback
            migrationBuilder.AddColumn<int>(
                name: "TeamsTeamId",
                table: "Employees",
                nullable: true); // Adjust nullable according to your needs
        }
    }
}
