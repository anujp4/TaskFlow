using AutoMapper;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Core.Entities;

namespace TaskFlow.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // Task mappings
            CreateMap<WorkspaceTask, TaskDto>()
                .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateTaskDto, WorkspaceTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore());

            CreateMap<UpdateTaskDto, WorkspaceTask>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore());
        }
    }
} 