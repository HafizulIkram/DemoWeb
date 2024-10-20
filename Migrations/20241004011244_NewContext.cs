using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class NewContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTask_Employees_EmployeeId",
                table: "EmployeeTask");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTask_Tasks_TaskId",
                table: "EmployeeTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeTask",
                table: "EmployeeTask");

            migrationBuilder.RenameTable(
                name: "EmployeeTask",
                newName: "EmployeeTasks");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTask_TaskId",
                table: "EmployeeTasks",
                newName: "IX_EmployeeTasks_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTask_EmployeeId",
                table: "EmployeeTasks",
                newName: "IX_EmployeeTasks_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeTasks",
                table: "EmployeeTasks",
                column: "EmploTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTasks_Employees_EmployeeId",
                table: "EmployeeTasks",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTasks_Tasks_TaskId",
                table: "EmployeeTasks",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTasks_Employees_EmployeeId",
                table: "EmployeeTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTasks_Tasks_TaskId",
                table: "EmployeeTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeTasks",
                table: "EmployeeTasks");

            migrationBuilder.RenameTable(
                name: "EmployeeTasks",
                newName: "EmployeeTask");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTasks_TaskId",
                table: "EmployeeTask",
                newName: "IX_EmployeeTask_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTasks_EmployeeId",
                table: "EmployeeTask",
                newName: "IX_EmployeeTask_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeTask",
                table: "EmployeeTask",
                column: "EmploTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTask_Employees_EmployeeId",
                table: "EmployeeTask",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTask_Tasks_TaskId",
                table: "EmployeeTask",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
