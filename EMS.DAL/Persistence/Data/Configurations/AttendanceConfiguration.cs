using EMS.DAL.Entities.AttendanceEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.DAL.Persistence.Data.Configurations
{
    public class AttendanceConfiguration : BaseEntityConfiguration<Attendance>, IEntityTypeConfiguration<Attendance>
    {
        public new void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.Property(a => a.Date).HasColumnType("date");
            builder.Property(a => a.Notes).HasColumnType("varchar(250)");
            builder.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
            builder.HasIndex(a => a.EmployeeId);
            builder.HasIndex(a => a.Date);

            base.Configure(builder);
        }
    }
}
