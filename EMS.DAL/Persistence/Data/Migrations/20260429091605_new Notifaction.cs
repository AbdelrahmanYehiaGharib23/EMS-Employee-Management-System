using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DAL.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class newNotifaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EmployeeProfileRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "EmployeeProfileRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "EmployeeProfileRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeType",
                table: "EmployeeProfileRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "EmployeeProfileRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringTime",
                table: "EmployeeProfileRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarred",
                table: "EmployeeProfileRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "EmployeeProfileRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "EmployeeProfileRequests",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "EmployeeType",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "HiringTime",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "IsMarred",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "EmployeeProfileRequests");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "EmployeeProfileRequests");
        }
    }
}
