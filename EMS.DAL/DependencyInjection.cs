
using EMS.DAL.Contracts;
using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Interfaces;
using EMS.DAL.Persistence.Data.DbInitializer;
using EMS.DAL.Persistence.Repositories;
using EMS.DAL.Persistence.UnitOfWork;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMS.DAL
{
    public static  class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.UseLazyLoadingProxies();
                       
            });

            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));  //Because DI doesn't know what type of T it will use.
            services.AddScoped<IDbInitalizer, DbInitializer>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
