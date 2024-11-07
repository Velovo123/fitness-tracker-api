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
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

            CreateMap<ProgressRecordDto, ProgressRecord>()
                .ForMember(dest => dest.ExerciseId, opt => opt.Ignore()) 
                .ForMember(dest => dest.Exercise, opt => opt.Ignore()); 
        }
    }
}
