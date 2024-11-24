using AutoMapper;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Mappings
{
    public class WorkoutMappingProfile : Profile
    {
        public WorkoutMappingProfile()
        {
            CreateMap<Workout, WorkoutDto>()
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.WorkoutExercises))
                .ReverseMap();

            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Exercise.Category ?? "Unknown"))
                .ReverseMap()
                .ForPath(dest => dest.Exercise.Name, opt => opt.MapFrom(src => src.ExerciseName))
                .ForPath(dest => dest.Exercise.Category, opt => opt.MapFrom(src => src.Category));
        }
    }
}
    