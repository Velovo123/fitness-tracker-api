using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutExerciseDto(
        [Required(ErrorMessage = "Exercise name is required.")]
        [MaxLength(100, ErrorMessage = "Exercise name cannot exceed 100 characters.")]
        string ExerciseName,

        [Required(ErrorMessage = "Sets are required.")]
        [Range(1, 100, ErrorMessage = "Sets must be between 1 and 100.")]
        int Sets,

        [Required(ErrorMessage = "Reps are required.")]
        [Range(1, 100, ErrorMessage = "Reps must be between 1 and 100.")]
        int Reps,

        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters.")]
        string? Type = null
    );
}