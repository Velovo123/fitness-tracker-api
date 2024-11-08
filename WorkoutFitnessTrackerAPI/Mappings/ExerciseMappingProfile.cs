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
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => NameNormalizationHelper.NormalizeName(src.Name)))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore Id to avoid duplicate entities
        }
    }
}
