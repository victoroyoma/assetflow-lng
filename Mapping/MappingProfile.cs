using AutoMapper;
using buildone.Data;
using buildone.DTOs;

namespace buildone.Mapping;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Asset mappings
        CreateMap<Asset, AssetResponseDto>()
            .ForMember(dest => dest.AssignedEmployee, opt => opt.MapFrom(src => src.AssignedEmployee))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department));
        
        CreateMap<CreateAssetDto, Asset>();
        CreateMap<UpdateAssetDto, Asset>();

        // Employee mappings
        CreateMap<Employee, EmployeeBasicDto>();

        // Department mappings
        CreateMap<Department, DepartmentBasicDto>();

        // User mappings
        CreateMap<ApplicationUser, UserResponseDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Populated separately
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src.Employee));
        
        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore());
    }
}
