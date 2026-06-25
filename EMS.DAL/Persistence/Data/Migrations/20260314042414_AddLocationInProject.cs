using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DAL.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationInProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInsideCompany",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Attendances",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInsideCompany",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Attendances");
        }
    }
}
