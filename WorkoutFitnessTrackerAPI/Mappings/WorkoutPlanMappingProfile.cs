using AutoMapper;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Mappings
{
    public class WorkoutPlanMappingProfile : Profile
    {
        public WorkoutPlanMappingProfile()
        {
            CreateMap<WorkoutPlan, WorkoutPlanDto>()
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src =>
                    src.WorkoutPlanExercises.Select(wpe => new WorkoutPlanExerciseDto
                    {
                        ExerciseName = wpe.Exercise.Name,
                        Sets = wpe.Sets,
                        Reps = wpe.Reps
                    }).ToList()));

            CreateMap<WorkoutPlanExercise, WorkoutPlanExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets))
                .ForMember(dest => dest.Reps, opt => opt.MapFrom(src => src.Reps));
        }
    }
}
