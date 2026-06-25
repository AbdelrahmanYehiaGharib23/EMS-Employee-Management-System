using EMS.DAL.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.DAL.Persistence.Data.Configurations
{
    public class EmployeeProfileRequestConfiguration : BaseEntityConfiguration<EmployeeProfileRequest>, IEntityTypeConfiguration<EmployeeProfileRequest>
    {
        public new void Configure(EntityTypeBuilder<EmployeeProfileRequest> builder)
        {
            builder.Property(r => r.Salary).HasPrecision(18, 2);

            base.Configure(builder);
        }
    }
}
