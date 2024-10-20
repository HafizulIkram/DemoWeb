﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class PleaseWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "teamsTeamId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_teamsTeamId",
                table: "Employees",
                column: "teamsTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Teams_teamsTeamId",
                table: "Employees",
                column: "teamsTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Teams_teamsTeamId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_teamsTeamId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "teamsTeamId",
                table: "Employees");
        }
    }
}
