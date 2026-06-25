using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities.Location;
using EMS.DAL.Persistence.Data.DbInitializer;

namespace EMS.DAL.Persistence.Repositories
{
    public class CompanyLocationRepository : GenericRepository<CompanyLocation>, ICompanyLocationRepository
    {
        public CompanyLocationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
