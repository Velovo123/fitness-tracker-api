namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record PeriodComparisonDto
    {
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }
        public int TotalWorkouts { get; init; }
        public double AverageDuration { get; init; }
        public int TotalReps { get; init; }
        public int TotalSets { get; init; }
    }
}
