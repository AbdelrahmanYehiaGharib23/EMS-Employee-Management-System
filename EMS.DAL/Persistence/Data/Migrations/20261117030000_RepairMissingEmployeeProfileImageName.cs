using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DAL.Persistence.Data.Migrations
{
    /// <summary>
    /// Repairs databases where the earlier ImageName migration is recorded as applied
    /// but the EmployeeProfileRequests column is missing from the physical schema.
    /// </summary>
    public partial class RepairMissingEmployeeProfileImageName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[EmployeeProfileRequests]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.EmployeeProfileRequests', N'ImageName') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[EmployeeProfileRequests]
                        ADD [ImageName] nvarchar(max) NULL;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep the column: the original migration owns it, and it may contain user data.
        }
    }
}
