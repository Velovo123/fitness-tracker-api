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
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.WorkoutPlanExercises))
                .ReverseMap()
                .ForMember(dest => dest.WorkoutPlanExercises, opt => opt.MapFrom(src => src.Exercises));

            CreateMap<WorkoutPlanExercise, WorkoutPlanExerciseDto>().ReverseMap();
        }
    }
}
