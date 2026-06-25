using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DAL.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfCreation",
                table: "Departments",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfCreation",
                table: "Departments");
        }
    }
}
