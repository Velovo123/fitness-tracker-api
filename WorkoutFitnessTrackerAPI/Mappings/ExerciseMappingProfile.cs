using AutoMapper;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTracker.API.Mappings
{
    public class ExerciseProfile : Profile
    {
        public ExerciseProfile()
        {
            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => NameNormalizationHelper.NormalizeName(src.Name)))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); 
        }
    }
}
