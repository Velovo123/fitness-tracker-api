namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record ExerciseProgressTrendDto
    {
        public DateTime Date { get; init; } 
        public string Progress { get; init; } 
    }
}
