using System.ComponentModel.DataAnnotations;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutPlanExerciseDto : IExerciseDto
    {
        [Required(ErrorMessage = "Exercise name is required.")]
        [MaxLength(100, ErrorMessage = "Exercise name cannot exceed 100 characters.")]
        public string ExerciseName { get; init; } = string.Empty;

        [Required(ErrorMessage = "Sets are required.")]
        [Range(1, 100, ErrorMessage = "Sets must be between 1 and 100.")]
        public int Sets { get; init; }

        [Required(ErrorMessage = "Reps are required.")]
        [Range(1, 100, ErrorMessage = "Reps must be between 1 and 100.")]
        public int Reps { get; init; }
    }
}
