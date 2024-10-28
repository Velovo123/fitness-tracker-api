namespace WorkoutFitnessTrackerAPI.Models.Dto_s
{
    public class WorkoutPlanQueryParams
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 10;

        public string? SortBy { get; set; } 
        public bool? SortDescending { get; set; } = false;

        public string? Goal { get; set; } 
    }
}
