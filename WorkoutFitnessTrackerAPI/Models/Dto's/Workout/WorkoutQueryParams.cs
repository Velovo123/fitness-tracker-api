using System.ComponentModel.DataAnnotations;

namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutQueryParams
    {
        public int? MinDuration { get; init; }
        public int? MaxDuration { get; init; }
        [DataType(DataType.Date, ErrorMessage = "Invalid start date format.")]
        public DateTime? StartDate { get; init; }

        [DataType(DataType.Date, ErrorMessage = "Invalid end date format.")]
        public DateTime? EndDate { get; init; }

        [RegularExpression("^(duration|date)$", ErrorMessage = "SortBy must be either 'duration' or 'date'.")]
        public string? SortBy { get; init; } 
        public bool? SortDescending { get; init; } = false;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1.")]
        public int? PageNumber { get; init; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int? PageSize { get; init; } = 10;
    }
}
