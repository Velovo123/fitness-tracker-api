namespace WorkoutFitnessTracker.API.Models.Dto_s.Summary
{
    public record ProgressSummaryDto
    {
        public int TotalWorkouts { get; init; }
        public int TotalDuration { get; init; } 
        public int UniqueExercises { get; init; }
        public Dictionary<string, int> ExerciseFrequency { get; init; } = new(); 
        public List<string> Achievements { get; init; } = new(); 
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
