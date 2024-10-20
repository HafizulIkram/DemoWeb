using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class DropTeamsTableAndRemoveTeamsTeamId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint first
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Teams_teamsTeamId",
                table: "Employees");

            // Now drop the Teams table
            migrationBuilder.DropTable(
                name: "Teams");

            // Finally, drop the TeamsTeamId column from Employees table
            migrationBuilder.DropColumn(
                name: "TeamsTeamId",
                table: "Employees");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the Teams table in case of rollback
            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                });

            // Re-add the TeamsTeamId column to Employees table
            migrationBuilder.AddColumn<int>(
                name: "TeamsTeamId",
                table: "Employees",
                nullable: true);

            // Re-add the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Teams_teamsTeamId",
                table: "Employees",
                column: "TeamsTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict); // Adjust according to your needs
        }
    }
}
