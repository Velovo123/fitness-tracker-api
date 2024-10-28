using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;

namespace WorkoutFitnessTrackerAPI.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))  
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
