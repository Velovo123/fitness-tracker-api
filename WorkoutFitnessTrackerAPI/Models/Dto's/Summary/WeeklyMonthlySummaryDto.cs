namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record WeeklyMonthlySummaryDto
    {
        public int TotalWorkouts { get; init; }
        public double AverageDuration { get; init; }
        public int TotalReps { get; init; }
        public int TotalSets { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

}
