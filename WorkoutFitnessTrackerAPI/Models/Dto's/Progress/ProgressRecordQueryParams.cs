namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record ProgressRecordQueryParams
    {
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
        public string? SortBy { get; init; } = "date"; 
        public bool? SortDescending { get; init; } = false;
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? ExerciseName { get; init; }
    }
}
