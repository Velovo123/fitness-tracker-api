using AutoMapper;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Mappings
{
    public class ProgressRecordMappingProfile : Profile
    {
        public ProgressRecordMappingProfile()
        {
            CreateMap<ProgressRecord, ProgressRecordDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress));
        }
    }
}
