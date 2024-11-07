using AutoMapper;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Mappings
{
    public class WorkoutPlanMappingProfile : Profile
    {
        public WorkoutPlanMappingProfile()
        {
            CreateMap<WorkoutPlan, WorkoutPlanDto>().ReverseMap();
            CreateMap<WorkoutPlanExercise, WorkoutPlanExerciseDto>().ReverseMap();
        }
    }
}
