using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DAL.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageNameToEmployeeProfileRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "EmployeeProfileRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "EmployeeProfileRequests");
        }
    }
}
