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
                .ReverseMap()
                .ForMember(dest => dest.WorkoutExercises, opt => opt.Ignore()); // Avoid recursive mapping issues

            // Bidirectional mapping between WorkoutExercise and WorkoutExerciseDto
            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Exercise.Type ?? "Unknown"))
                .ReverseMap()
                .ForPath(dest => dest.Exercise.Name, opt => opt.MapFrom(src => src.ExerciseName))
                .ForPath(dest => dest.Exercise.Type, opt => opt.MapFrom(src => src.Type));
        }
    }
}
