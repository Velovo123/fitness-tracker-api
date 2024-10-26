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
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src =>
                    src.WorkoutExercises.Select(we => new WorkoutExerciseDto
                    {
                        ExerciseName = we.Exercise.Name,
                        Sets = we.Sets,
                        Reps = we.Reps,
                        Type = we.Exercise.Type ?? "Unknown"
                    }).ToList()
                ));

            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Exercise.Type))
                .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets))
                .ForMember(dest => dest.Reps, opt => opt.MapFrom(src => src.Reps));
        }
    }
}
