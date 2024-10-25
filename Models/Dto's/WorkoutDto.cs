using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutDto(
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date format.")]
        DateTime Date,

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes.")]
        int Duration,

        [Required(ErrorMessage = "At least one exercise is required.")]
        [MinLength(1, ErrorMessage = "At least one exercise must be provided.")]
        List<WorkoutExerciseDto> Exercises
    );
}
