namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public record WorkoutQueryParams
    {
        public int? MinDuration { get; init; }
        public int? MaxDuration { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? SortBy { get; init; } 
        public bool? SortDescending { get; init; } = false;
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
    }
}
