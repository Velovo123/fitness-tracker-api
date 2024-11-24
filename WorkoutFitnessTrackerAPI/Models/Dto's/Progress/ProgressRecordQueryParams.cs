using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record ProgressRecordQueryParams
    {
        [Range(1, int.MaxValue)]
        public int? PageNumber { get; init; } = 1;

        [Range(1, 100)]
        public int? PageSize { get; init; } = 10;
        public string? SortBy { get; init; } = "date"; 
        public bool? SortDescending { get; init; } = false;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; init; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; init; }

        [MaxLength(100)]
        public string? ExerciseName { get; init; }
    }
}
