using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record ProgressRecordDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; init; }
        [Required]
        [MaxLength(100)]
        public string Progress { get; init; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ExerciseName { get; init; } = string.Empty;
    }

}
