using AutoMapper;
using EMS.BLL.Models.Dto.EmployeeDto;
using EMS.PL.ViewModels.Employee;

namespace EMS.PL.Mapping
{
    public class PresentationProfile:Profile
    {
        public PresentationProfile()
        {
            CreateMap<EmployeeDetailsDto, EmployeeViewModel>()
               .ForMember(dest => dest.DepartmentId,
                   opt => opt.MapFrom(src => src.DepartmentId));


            CreateMap<EmployeeViewModel, UpdateEmployeeDto>()
                 .ForMember(dest => dest.DepartmentId,
                     opt => opt.MapFrom(src => src.DepartmentId));

        }
    }
}
