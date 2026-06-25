using AutoMapper;
using EMS.BLL.Models.Dto.EmployeeDto;
using EMS.DAL.Entities.EmployeeEntity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Employee,EmployeeDto>()
                .ForMember(dest=>dest.EmployeeType,options=>options.MapFrom(src=>src.EmployeeType))
                .ForMember(dest=>dest.Gender,options=>options.MapFrom(src=>src.Gender))
                .ForMember(dest => dest.HiringTime, options => options.MapFrom(src => DateOnly.FromDateTime(src.HiringTime)))
                 .ForMember(dest => dest.DepartmentName,opt=>opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

            CreateMap<Employee,EmployeeDetailsDto>()
                .ForMember(dest => dest.EmployeeType, options => options.MapFrom(src => src.EmployeeType))
                .ForMember(dest => dest.Gender, options => options.MapFrom(src => src.Gender))
                .ForMember(dest=>dest.HiringTime,options=>options.MapFrom(src=>DateOnly.FromDateTime(src.HiringTime)))
                 .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

            CreateMap<CreateEmployeeDto, Employee>()
              .ForMember(dest => dest.HiringTime,
                    opt => opt.MapFrom(src => src.HiringTime.ToDateTime(TimeOnly.MinValue)))
              .ForMember(dest => dest.ImageName,
                    opt => opt.MapFrom(src => src.ImageName));

            CreateMap<UpdateEmployeeDto, Employee>()
                .ForMember(dest => dest.HiringTime,
                    opt => opt.MapFrom(src => src.HiringTime.ToDateTime(TimeOnly.MinValue)));

        }
    }
}
