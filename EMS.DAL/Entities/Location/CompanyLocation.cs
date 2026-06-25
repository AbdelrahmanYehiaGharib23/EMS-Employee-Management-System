using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Entities.Location
{
    public class CompanyLocation:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int AllowedRadiusMeters { get; set; }
    }
}
