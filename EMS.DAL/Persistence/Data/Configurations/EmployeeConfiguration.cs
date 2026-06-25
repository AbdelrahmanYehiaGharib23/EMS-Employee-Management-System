using EMS.DAL.Entities.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Persistence.Data.Configurations
{
    public class EmployeeConfiguration : BaseEntityConfiguration<Employee>, IEntityTypeConfiguration<Employee>
    {
        public new  void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(E=>E.Name).HasColumnType("varchar(50)");
            builder.Property(E => E.Address).HasColumnType("varchar(150)");
            builder.Property(E => E.Salary).HasColumnType("decimal(18,2)");

            builder.Property(E => E.Gender).HasConversion((EmpGender) => EmpGender.ToString(),
                (_gender) => (Gender)Enum.Parse(typeof(Gender), _gender));

            builder.Property(E => E.EmployeeType).HasConversion((EmpType) => EmpType.ToString(),
                (_type) => (EmployeeType)Enum.Parse(typeof(EmployeeType), _type));
            base.Configure(builder);
        }
    }
}
