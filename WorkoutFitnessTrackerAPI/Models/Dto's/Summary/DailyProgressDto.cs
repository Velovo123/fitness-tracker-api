namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record DailyProgressDto
    {
        public DateTime Date { get; init; }
        public int TotalWorkouts { get; init; }
        public double AverageDuration { get; init; }
        public int TotalReps { get; init; }
        public int TotalSets { get; init; }
        public List<ExerciseProgressDto> Exercises { get; init; } = new();
    }
}
