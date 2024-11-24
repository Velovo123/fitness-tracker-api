using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutPlanDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; init; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Goal cannot exceed 100 characters")]
        public string Goal { get; init; } = "Not specified";

        [Required(ErrorMessage = "At least one exercise is required")]
        [MinLength(1, ErrorMessage = "At least one exercise must be provided")]
        public List<WorkoutPlanExerciseDto> Exercises { get; init; } = new();
    }
}
