using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class TestDeleteTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Teams_TeamsTeamId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Employees_TeamsTeamId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TeamsTeamId",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamsTeamId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamRole = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TeamsTeamId",
                table: "Employees",
                column: "TeamsTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Teams_TeamsTeamId",
                table: "Employees",
                column: "TeamsTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");
        }
    }
}
